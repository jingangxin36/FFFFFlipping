## FFFFFlipping

此项目仅用于个人学习(●'◡'●)

### 游戏截图

[待补充]>>>emm录制gif的时候出错了...

### APK下载

[FFFFFlipping-V1.1.apk](https://github.com/jingangxin36/FFFFFlipping/releases/download/V1.1/FFFFFlipping-V1.1.apk)

### 开发环境

 - Unity 2018.1

### 主要技术

#### 相机跟随

**效果图**:
![](https://github.com/jingangxin36/FFFFFlipping/blob/master/FFFFFlipping/Demo/4.gif)

**具体实现**:

- [ Unity 实现人物相机前后跟随, 带延迟效果 ](https://blog.csdn.net/jingangxin666/article/details/80557766)

#### 无限地图

**效果图:**
![](https://github.com/jingangxin36/FFFFFlipping/blob/master/FFFFFlipping/Demo/5.gif)

#### 敌人围绕

//todo

#### 重载场景后灯光变暗问题处理

##### 原因：

- 选择的光照是GI realtime实时光照，编辑器在当前场景时，它的灯光是已经渲染好了，但重新加载的时候灯光没有进行渲染

##### 解决方法：

- Window>>lighting>>settings>>右下角取消勾选auto，这时候是没有烘焙灯光的情形，重新加载场景后不再会变暗。
-  如果需要烘培灯光，则点击Generate按钮即可，这时候将保存光照贴图信息，重新加载后也不会再变暗。

##### 解决方法参考:

- [Unity 2017 重新载入场景与灯光变暗问题处理](https://www.jianshu.com/p/6f7891a521d0?utm_campaign=maleskine&utm_content=note&utm_medium=seo_notes&utm_source=recommendation) 



### 为什么叫做FFFFFlipping呢? 

纯属因为个人喜欢的两款放荡不羁的游戏

呀哈哈哈

(1)[Eggggg（喷蛋狂人）](https://itunes.apple.com/cn/app/eggggg-%E5%96%B7%E8%9B%8B%E7%8B%82%E4%BA%BA-%E5%B9%B3%E5%8F%B0%E5%91%95%E5%90%90%E6%B8%B8%E6%88%8F/id1145738671?mt=8)

(2)[Flipping Legend (弹跳传奇)](https://itunes.apple.com/cn/app/flipping-legend/id1218046599?mt=8)

### 个人博客

[jingangxin36](https://blog.csdn.net/jingangxin666)

### 个人邮箱

jingangxin36@foxmail.com


