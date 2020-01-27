# Octree

## Requirements
  
* Built for DOTS. Contiguous memory that can be easily processed.
* Built for scale. Should be able to write and read parts of the Octree to disk so that the Octree can be bigger then available memory.

## Structure

Rather then nodes containing pointers to their children, each layer of the Octree is stored in a sparse array. This mean nodes can be stored in a tightly packed array, ready for processing.

Level 0 of the Octree is a single node, level 1 has 8 nodes, level 2 has 64, etc.

### Indexing

Each node coordinate should be easily convertible to an array index and back again. The child indexes of a node should be easily calculated. Child nodes should be in contiguous memory, so they can be referred to by index range.

A node is defined by a level and a 3D coordinate. The 3D coordinate is converted to an index using a space filling curve. This means the child nodes can be contiguous. IE In level 2 the index range 0-7 is the children of the first node, 8-15 are the children of node 2, etc.

Z-order (Morton, Lebesgue) curves are good choice for simplicity and performance. A Hilbert curve give you spatially adjacent nodes, so index i and i+1 will always be a next door neighbour Octree node. We really just want contiguous nodes and the convenience of being able to pull out a group of the 8 child nodes as a index range. So there is no justification for the code complexity and performance cost of a Hilbert curve.

https://www.forceflow.be/2013/10/07/morton-encodingdecoding-through-bit-interleaving-implementations/
https://github.com/aavenel/mortonlib


This is a [good visual introduction to Lebesgue curves](https://www.robertdickau.com/lebesgue3d.html) and here is [Eddie Woo explaining the concept](https://www.youtube.com/watch?v=77I0ic8cBw0). We're after a way to transform a 3d coordinate (node position) into a 1d coordinate (array index) and back again.

This is simply done using 2 stacked "N"'s. The coordinates of level 1 of the Octree are:
 (0,0,0), (0,0,1), (1,0,0), (1,0,1)
 (0,1,0), (0,1,1), (1,1,0), (1,1,1)

And they become indices 0 through to 7.

/// OUT OF DATE
The curve is fractal. When we go to level 2 node there are 64 nodes, each of the 8 nodes has 8 children. The start of the index ranges for the children of each parent node are [0,7,15,23,31,39,47,55]. So for this level, given the current index 'i', to find the start index of a child node using *i \* 8 - 1*.

At level 3 each of these 64 nodes has 8 children, giving 512 nodes. The children of node (0,0,0) from level 2 will have the same ranges as level 2, and include the indices [0-63]. The children of node (0,0,1) will be contained in the range [64-127]. So given the parent index 'i' and the number of nodes in a level 'n' we can find the start of the child indices using *i \* n*.

You can find the number of nodes in a level 'l' using *8 ^ l*. 

Putting it all together, given a node's index, the start of the child index range can be found using *i \* (8 ^ l )*.
/// OUT OF DATE

### Sparse Array

A double-indirection. Due to the large number of nodes in a level, which mostly wont be filled, the nodes are stored in a compact array and another array stores sorted indices into the node array. To access a node a binary search is performed on the index array.

## License

MIT License