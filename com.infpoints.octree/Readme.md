# Octree

## Requirements
  
* Built for DOTS. Contiguous memory that can be easily processed.
* Built for scale. Should be able to write and read parts of the Octree to disk so that the Octree can be bigger then available memory.

## Structure

Rather then nodes containing pointers to their children, each layer of the Octree is stored in a sparse array. This mean nodes can be stored in a tightly packed array, ready for processing.

Level 0 of the Octree is a single node, level 1 has 8 nodes, level 2 has 64, etc.

### Indexing

Each node should be easily convertible to an array index and back again. The child indexes of a node should be easily calculated. Child nodes should be in contiguous memory, so they can be referred to by index range.

A node is defined by a level and a 3D coordinate. The 3D coordinate is converted to an index using a space filling curve. This means the child nodes can be contiguous. IE In level 2 the index range 0-7 is the children of the first node, 8-15 are the children of node 2, etc.

Lebesgue curves are good choice for simplicity and performance.

### Sparse Array

A double-indirection. Due to the large number of nodes in a level, the nodes are stored in a compact array and another array stores sorted indices into the node array. To access a node a binary search is performed on the index array.

