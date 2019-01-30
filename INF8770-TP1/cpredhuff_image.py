# Main source code : 
# Predictive coding : Gabriel Bilodeau https://github.com/gabilodeau/INF8770/blob/master/Codage%20predictif%20sur%20image.ipynb
# Huffman enconding : Joshua Ebenezer https://github.com/JoshuaEbenezer/huffman_encoding/blob/master/README.md

import time
import numpy as np
from scipy.misc import imread,imresize
import matplotlib.pyplot as plt
from operator import itemgetter, attrgetter
import queue

class Node:
	def __init__(self):
		self.prob = None
		self.code = None
		self.data = None
		self.left = None
		self.right = None 	# the color (the bin value) is only required in the leaves
	def __lt__(self, other):
		if (self.prob < other.prob):		# define rich comparison methods for sorting in the priority queue
			return 1
		else:
			return 0
	def __ge__(self, other):
		if (self.prob > other.prob):
			return 1
		else:
			return 0

def rgb2gray(img):
	gray_img = np.rint(img[:,:,0]*0.2989 + img[:,:,1]*0.5870 + img[:,:,2]*0.1140)
	gray_img = gray_img.astype(int)
	return gray_img

def get2smallest(data):			# can be used instead of inbuilt function get(). was not used in  implementation
    first = second = 1
    fid=sid=0
    for idx,element in enumerate(data):
        if (element < first):
            second = first
            sid = fid
            first = element
            fid = idx
        elif (element < second and element != first):
            second = element
    return fid,first,sid,second
    
def tree(probabilities):
	prq = queue.PriorityQueue()
	for color,probability in enumerate(probabilities):
		leaf = Node()
		leaf.data = color
		leaf.prob = probability
		prq.put(leaf)

	while (prq.qsize()>1):
		newnode = Node()		# create new node
		l = prq.get()
		r = prq.get()			# get the smalles probs in the leaves
						# remove the smallest two leaves
		newnode.left = l 		# left is smaller
		newnode.right = r
		newprob = l.prob+r.prob	# the new prob in the new node must be the sum of the other two
		newnode.prob = newprob
		prq.put(newnode)	# new node is inserted as a leaf, replacing the other two 
	return prq.get()		# return the root node - tree is complete

def huffman_traversal(root_node,tmp_array,f):		# traversal of the tree to generate codes
	if (root_node.left is not None):
		tmp_array[huffman_traversal.count] = 1
		huffman_traversal.count+=1
		huffman_traversal(root_node.left,tmp_array,f)
		huffman_traversal.count-=1
	if (root_node.right is not None):
		tmp_array[huffman_traversal.count] = 0
		huffman_traversal.count+=1
		huffman_traversal(root_node.right,tmp_array,f)
		huffman_traversal.count-=1
	else:
		huffman_traversal.output_bits[root_node.data] = huffman_traversal.count		#count the number of bits for each color
		bitstream = ''.join(str(cell) for cell in tmp_array[1:huffman_traversal.count]) 
		color = str(root_node.data)
		wr_str = color+' '+ bitstream+'\n'
		f.write(wr_str)		# write the color and the code to a file
	return

def predicteur(nAlgo, imagetocompress):
	erreur = np.zeros((len(imagetocompress)-2,len(imagetocompress[0])-2))
	imagepred = np.zeros((len(imagetocompress)-2,len(imagetocompress[0])-2))
	if(nAlgo == 1):
		print('Prediction matrix')
		# prediction matrix
		matpred = [[0.33,0.33],[0.33,0.0]]
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=imagetocompress[i-1][j-1]*matpred[0][0]+imagetocompress[i-1][j]*matpred[0][1]+imagetocompress[i][j-1]*matpred[1][0]
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 2):
		print('jpeg4')
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=imagetocompress[i][j-1]+imagetocompress[i-1][j]-imagetocompress[i-1][j-1]
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 3):
		print('jpeg7')
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=(imagetocompress[i][j-1]+imagetocompress[i-1][j])/2
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 4):
		print('p2')
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=imagetocompress[i][j-1]+(imagetocompress[i-1][j+1]-imagetocompress[i-1][j-1])/2
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 5):
		print('p3')
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=(0.66)*(imagetocompress[i][j-1]+imagetocompress[i-1][j])-(0.33)*imagetocompress[i-1][j-1]
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 6):
		print('pr2')
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=(2*imagetocompress[i][j-1]+imagetocompress[i-1][j])/3
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	if(nAlgo == 7):
		print('heuristic - test')
		matpred = [[0.25,0.25],[0.25,0.25]]
		for i in range(1,len(imagetocompress)-2):
			for j in range(1,len(imagetocompress[0])-2):
				imagepred[i][j]=imagetocompress[i-1][j-1]*matpred[0][0]+imagetocompress[i-1][j]*matpred[0][1]+imagetocompress[i][j-1]*matpred[1][0]+imagetocompress[i-1][j+1]*matpred[1][1]
				erreur[i][j]=imagepred[i][j]-imagetocompress[i][j]
	return erreur.astype('uint8')

input = input("Enter file to process: ")

# Opens the file
try:
	imagelue = imread(input)
	image=imagelue.astype('float')
except IOError:
	print("Can't read input file.")
	exit(2)

for p in range(1,8):
	start = time.time()

	# convert a rgb image to gray
	image=imagelue.astype('float')
	image=rgb2gray(image)

	# compute histogram of pixels
	input_bits = image.shape[0]*image.shape[1]*8	# calculate number of bits in grayscale 

	# double images borders to have pixels to predict the images borders 
	col=image[:,0]
	image = np.column_stack((col,image))
	col=image[:,len(image[0])-1]
	image = np.column_stack((col,image))
	row=image[0,:]
	image = np.row_stack((row,image))
	row=image[len(image)-1,:]
	image = np.row_stack((row,image))

	# calculate images predictions and errors
	img=predicteur(p,image)

	# compute histogram of pixels
	hist = np.bincount(img.ravel(),minlength=256)

	probabilities = hist/np.sum(hist)		# a priori probabilities from frequencies

	root_node = tree(probabilities)			# create the tree using the probs.
	tmp_array = np.ones([128],dtype=int)
	huffman_traversal.output_bits = np.empty(256,dtype=int) 
	huffman_traversal.count = 0
	f = open('codes.txt','w')
	huffman_traversal(root_node,tmp_array,f)		# traverse the tree and write the codes

	end = time.time()

	compression = (1-np.sum(huffman_traversal.output_bits*hist)/input_bits)*100	# compression rate
	timeCompression = end - start  # compression time of combined methods
	print('Compression rate is ',round(compression, 3),' %')
	print('Time to compress ',round(timeCompression, 3),' seconds')
	print('')