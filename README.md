八方向A*寻路

路径优化：

1.共线点删除，降低后续优化复杂度。

2.Los+Bresenham路径平滑。

3.去首点避免寻路启动时与整体方向相反。


walkable检测方式： 半径扩张。


A*使用网格坐标，真实寻路用世界坐标，二者可转换，格子大小可随意配置。

最小堆优化+0GC。


后续计划：

1.减少A*扩展节点的数量，继续优化性能。（Theta* / Lazy Theta*）

2.大地图的Hierarchical A*实现。

3.walkalbe实现方案改用unity physic，做对比。
