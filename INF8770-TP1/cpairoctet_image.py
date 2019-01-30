#!/usr/bin/env python
# -*- coding: UTF-8 -*-
import time
import numpy as np
from scipy.misc import imread,imresize

# convert rgb image to gray and to bytes array
def rgb2gray(img):
	gray_img = np.rint(img[:,:,0]*0.2989 + img[:,:,1]*0.5870 + img[:,:,2]*0.1140)
	gray_img = gray_img.astype(int)
	return gray_img.tobytes()

# checks whether a character is usable for a definition
def char_usable(char, image):
	good = [9, 10, 13]; # "safe" characters with ordinals < 32
	if (char <= 255 and chr(char).encode('ascii', errors='replace') in image) or (char < 32 and char not in good):
		return False # not usable, try another one
	else: return True # usable
	
# Byte-pair compression
# Works by replacing common pairs of letters with a single letter (a "definition") and placing that definition in a dictionary
def compress(image):
	dictionary = [] # dictionary of pair definitions - very ironic
	dict_char = 9
	while not char_usable(dict_char, image): dict_char += 1 # find dictionary-delimiting character
	if dict_char > 255:
		print("Document uses all ASCII characters and is uncompressable.")
		exit(2)

	while True: # infinite loop
		character = 9
		while not char_usable(character, image) or character == dict_char: character += 1 # find character to define
		p = 0 # initialize pair pointer
		dict = {} # temp dictionary of pair frequencies
		while p < len(image): # loop through all possible pairs
			x = image[p:p+2]
			dict[x] = dict[x] + 1 if dict.get(x, False) else 1 # increment pair's frequency counter
			p += 1
		large = 0
		large_pair = ""
		for pair,amount in dict.items(): # check through dictionary for most frequent pair
			if amount > large:
				large_pair = pair
				large = amount # use it
		if large == 1 or character > 255: break # if there are no common pairs or we are out of characters to define, exit the loop
		image = image.replace(large_pair, chr(character).encode('ascii', errors='replace')) # perform the replacement with the definition
		dictionary.append([large_pair, chr(character).encode('ascii', errors='replace')]) # add the definition to the dictionary
	dict_str = chr(dict_char).encode('ascii', errors='replace') # get the dictionary-delimiting character
	for item in dictionary: # convert the dictionary into a string
		dict_str += item[0] + item[1]
	return chr(dict_char).encode('ascii', errors='replace') + image + dict_str # construct the final compressed string with all information necessary

# Byte-pair decompression
# reconstructs the dictionary, then applies it
def decompress(image):
	dictionary = [] # init dictionary
	dict_char = image[0] # find dictionary-delimiting character
	image = image[1:] # remove dict-delim char from image
	dict_start = image.index(dict_char)+1 # remove dicionary character from image
	dict_str = image[dict_start:] # separate out dictionary and image 
	image = image[:dict_start-1] # remove dictionary from image
	i = 0
	while i < len(dict_str): # loop and parse dictionary
		dictionary.append([dict_str[i:i+2], dict_str[i+2]])
		i += 3
	dictionary.reverse() # reverse it so items are read in right order
	for item in dictionary:
		image = image.replace(item[1], item[0]) # perform replacements
	return image

# Checks whether a file has been compressed using this script
def is_compressed(image):
	# first character in document (dictionary character) only appears elsewhere once: probably compressed using this algorithm 
	return True if image[1:].count(image[0]) == 1 else False  

choice = input("[c]ompress or [d]ecompress: ")
input = input("Enter file to process: ")

# Opens the file
try:
	imagelue = imread(input)
	image=imagelue.astype('float')
except IOError:
	print("Can't read input file.")
	exit(2)

# Runs selected option
if choice == "c":
	start = time.time()
	image=rgb2gray(image)
	result = compress(image)
	end = time.time()
	timeCompression = end - start
	print("----------")
	print("Original length:\t" + str(len(image)) + " bytes")
	print("Compressed length:\t" + str(len(result)) + " bytes (" + str(round((1 - float(len(result)) / float(len(image))) * 100, 2)) + "% compression rate)")
	print('Time to compress ',round(timeCompression, 3),' seconds')
	print ("----------")
elif is_compressed(image): result = decompress(image)
else:
	print("Input file not compressed!")
	exit()
	
#new_file = input("Enter output file (or leave blank to display): ")

# Writes to output file
#if len(new_file):
#	new = open(new_file, "wb")
#	new.write(result)
#	new.close()
#else: print(result)