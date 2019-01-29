#!/usr/bin/env python
# -*- coding: UTF-8 -*-
import time

from heapq import heappush, heappop, heapify
from collections import defaultdict
 
def encode(symb2freq):
    """Huffman encode the given dict mapping symbols to weights"""
    heap = [[wt, [sym, ""]] for sym, wt in symb2freq.items()]
    heapify(heap)
    while len(heap) > 1:
        lo = heappop(heap)
        hi = heappop(heap)
        for pair in lo[1:]:
            pair[1] = '0' + pair[1]
        for pair in hi[1:]:
            pair[1] = '1' + pair[1]
        heappush(heap, [lo[0] + hi[0]] + lo[1:] + hi[1:])
    return sorted(heappop(heap)[1:], key=lambda p: (len(p[-1]), p))
	
def write_file(result,text):
	compress_text = ""
	for char in text:
		for p in result:
			if p[0] == char:
				compress_text += p[1]
	return compress_text


choice = input("[c]ompress or [d]ecompress: ")
input = input("Enter file to process: ")

# Opens the file
try:
	file = open(input, "rb")
	text = file.read()
except IOError:
	print("Can't read input file.")
	exit(2)

# Runs selected option
if choice == "c":
	start = time.time()
	
	symb2freq = defaultdict(int)
	for char in text:
		symb2freq[char] += 1
		
	# symb2freq = collections.Counter(txt)
	result = encode(symb2freq)
	print ("Symbol\tWeight\tHuffman Code")
	for p in result:
		print ("%s\t%s\t%s" % (p[0], symb2freq[p[0]], p[1]))
		
	compress_text = write_file(result,text)
	
	#result = code_huffman(text)
	end = time.time()
	timeCompression = end - start
	print("----------")
	print("Original length:\t" + str(len(text)) + " bytes")
	print("Compressed length:\t" + str(len(compress_text)/8) + " bytes (" + str(round((float(len(compress_text)/8) / float(len(text))) * 100, 2)) + "% of original size)")
	print('Time to compress ',round(timeCompression, 3),' seconds')
	print ("----------")
elif is_compressed(text): compress_text = decompress(text)
else:
	print("Input file not compressed!")
	exit()