# Infinite Points

Version 0.000001

Unite Version 2019.3.0f3

It's becoming more and more common to use photogrammetry and lidar scanning to capture buildings, engineering projects, sites of cultural significance, archaeological digs, etc. Most of these points clouds are in the billions of points; slow to render and unable to fit into memory.

There are workflows for converting point cloud data into meshes, but the process is usually laborious and data is lost in the process. For digs or remote inspections keeping all the point cloud data is very important.

Infinite Points is a point cloud renderer for Unity that solves these issues but keeping parts of the point cloud data on disk and reading in the most important data for the users viewpoint.

## Requirements

* The data should be stored on disk in multiple files.
* These files should be able to be loaded into memory at runtime.
* The data should be laid out efficiently in memory to allow for quick processing, eg for culling.
* Use of Unity Native Containers and Burst were possible.

## Previous Work

* [Potree](http://www.potree.org/)
* [Unreal Point Cloud Plugin](https://pointcloudplugin.com/)

## Blog Posts
* [InfPoints - Introduction](http://johnsietsma.com/2019/11/29/infinite-points-introduction/)
* [Morton Order - Introduction](http://johnsietsma.com/2019/12/05/morton-order-introduction/)
* [Morton Order - Burst](http://johnsietsma.com/2019/12/13/mordon-burst/)

