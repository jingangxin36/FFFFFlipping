## FFFFFlipping

### 1. 游戏截图

[待补充]>>>emm录制gif的时候出错了...

### 2. APK下载

[FFFFFlipping-V1.1.apk](https://github.com/jingangxin36/FFFFFlipping/releases/download/V1.1/FFFFFlipping-V1.1.apk)

### 3. 怎么玩?

- 点键help按钮, 会显示操作提示
- 你可以向左跳, 向右跳, 每次只能跳一步
- 你可以向后跳一步来调整战略, 防止陷入僵局, 或者踩爆更多的敌人! 
- 你可以收集金币, 然后打破纪录! (但是现在还没有商店)
- 踩爆敌人获得加血, 踩到陷阱☠则会死亡
- 长时间没有加血, 呼吸和速度都会变慢! 然后死掉! 
- 开始弹跳之旅吧

### 4. 开发环境

 - Unity 2018.1

### 5. 技术关键点解析和拓展

#### 5.1 碰撞检测

**项目中的使用场景**

- **无限地图的拼接**: 通过在相机处设置碰撞体, 当相机看不见某一行block时, 将该block移到最后一排, 实现无限地图
- **cell(金币/敌人/陷阱☠)的处理:** 主角跳到敌人或陷阱的cell, 会有不同的反馈

**拓展**:

- [游戏引擎架构----物理部分](https://blog.csdn.net/peerlessbloom/article/details/72643368)

#### 5.2 相机跟随

**效果图**:
![](https://github.com/jingangxin36/FFFFFlipping/blob/master/FFFFFlipping/Demo/4.gif)

**实现细节**:

- 只做前后跟随, 即只需要z值跟随就好了
- 跟随有延迟效果,  采用 `Mathf.Lerp` 插值实现
- 具体操作介绍: [ Unity 实现人物相机前后跟随, 带延迟效果 ](https://blog.csdn.net/jingangxin666/article/details/80557766)

关键代码如下:

```csharp
using UnityEngine;
public class CameraMovement : MonoBehaviour {
    public GameObject followTarget;
    public float moveSpeed;
    void Update() {
        if (followTarget != null) {
            //相机位置Z值与目标点的Z值做插值, 实现相机前后跟随, 而目标点运动不影响
            var newZ = Mathf.Lerp(transform.position.z, followTarget.transform.position.z, Time.deltaTime * moveSpeed);
            var newVector3 = new Vector3(transform.position.x, transform.position.y, newZ);
            transform.position = newVector3;
        }
    }
}
```



#### 5.3 无限地图拼接

**效果图:**
![](https://github.com/jingangxin36/FFFFFlipping/blob/master/FFFFFlipping/Demo/5.gif)

**关键代码如下:**

```csharp
using UnityEngine;
public class BlockMovement : MonoBehaviour {
    void OnTriggerExit(Collider other) {
        //当相机看不见某一行block时, 将该block移到最后一排, 实现无限地图
        if (other.gameObject.tag == "Floor") {
            BlockManager.Instance.ChangeBlockPosition(other.transform);
        }
        if (other.gameObject.tag == "PoolItem") {
            var poolItem = other.transform.GetComponent<Cell>();
            poolItem.ReleaseIntoPool();
        }
    }
}
```

#### 5.4 敌人视觉模拟和角度跟随

敌人看到主角时, 会转向主角

**怎样算看到呢?** 项目中, 主角与敌人的z值距离小于指定值时, 可以断定主角进入敌人的视觉范围

**要怎么转向主角呢?** 用四元数来表示旋转, 同时限制欧拉角中的y值, 来限制旋转角度

**关键代码如下:**

```csharp
protected virtual bool SeesPlayer =>
        ((Player != null) && (Mathf.Abs((int)(transform.position.z - Player.transform.position.z)) <= visionRange));
//...
protected virtual void Update() {
    if (Player != null) {
        if (SeesPlayer) TurnTowardPlayer();
    }
}
//...
private void TurnTowardPlayer() {
    Vector3 position = Player.transform.position;
    //忽略y值的影响
    position.y = transform.position.y;
    Vector3 forward = transform.position - position;
    if (forward != Vector3.zero) {
        Quaternion b = Quaternion.LookRotation(forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, b, Time.deltaTime * turnRate);
        float y = transform.eulerAngles.y;
        //确定旋转角度
        if (y > 180f)  y -= 360f;
        //将旋转角度限制在-45度和+45度之间
        transform.rotation = Quaternion.Euler(0f, Mathf.Clamp(y, -mLookLimit, mLookLimit), 0f);
    }
}
```

**拓展:** 

- 3D数学中表现旋转的方式有:
  - **旋转矩阵:** 行列式为1的正交矩阵
    - 旋转矩阵**有9个量**，但是**一次旋转只有3个自由度**，因此这种表达方式是冗余的
    - 旋转矩阵自身带有约束：**它必须是个正交矩阵，且行列式为1**
  - **欧拉角:** 最直观的旋转描述方式，也是一个3维向量，分别代表绕某个轴的旋转角度
    - 相同的角度，旋转次序的不同，旋转结果不一样。一般常见的是rpy角（旋转顺序是ZYX）
    - 最大的缺点是万向锁问题：俯仰角为±90度时，第一次旋转和第三次旋转将使用同一个轴，使得系统丢失了一个自由度
    - 无法实现球面平滑插值
  - **四元数**: 四元数就是一个高阶复数，也就是一个四维空间
    - 它们结构紧凑
    - 不受万向节锁定的影响
    - 可提供球面平滑插值
    - 不够直观
    - Unity内部使用四元数来表示所有旋转
- **参考:**
  - [Unity学习笔记10——旋转（四元数和欧拉角）](https://blog.csdn.net/linshuhe1/article/details/51206377)
  - [三维空间刚体旋转描述](https://blog.csdn.net/zizi7/article/details/80763570) 

#### 5.5 主角跳跃穿越效果

**效果图为:**

![](https://github.com/jingangxin36/FFFFFlipping/blob/master/FFFFFlipping/Demo/1.png)

**关键代码为:**

```csharp
    /// <summary>
    /// 左右跳时, 主角也向前跳
    /// </summary>
    /// <param name="direction">-1表示向左跳, 1表示向右跳</param>
    private void ForwardJump(int direction) {
        mCurrentX += direction;
        if (direction == -1) {
            //越界了
            if (mCurrentX < 0) {
                mCurrentX = 2;
                var newVector3 = transform.position;
                newVector3.x = 2 * characterPositionOffset;
                transform.position = newVector3;
            }
            transform.Translate(Vector3.left * characterPositionOffset);
        }
        if (direction == 1) {
            //越界了
            if (mCurrentX > 2) {
                mCurrentX = 0;
                var newVector3 = transform.position;
                newVector3.x = -2 * characterPositionOffset;
                transform.position = newVector3;
            }
            transform.Translate(Vector3.right * characterPositionOffset);
        }
        JumpForward();
    }
```

#### 5.6 血量不足时的减速处理

```csharp
    public void LowBlood() {
        Time.timeScale = 0.5f;
        lowBloodView.SetActive(true);
    }

    public void ResumeNormalBlood() {
        Time.timeScale = 1;
        lowBloodView.SetActive(false);
    }
```

#### 5.7 重载场景后灯光变暗问题处理

##### 原因：

- 选择的光照是GI realtime实时光照，编辑器在当前场景时，它的灯光是已经渲染好了，但重新加载的时候灯光没有进行渲染

##### 解决方法：

- Window->lighting->settings->右下角取消勾选auto，这时候是没有烘焙灯光的情形，重新加载场景后不再会变暗。
-  如果需要烘培灯光，则点击Generate按钮即可，这时候将保存光照贴图信息，重新加载后也不会再变暗。

##### 解决方法参考:

- [Unity 2017 重新载入场景与灯光变暗问题处理](https://www.jianshu.com/p/6f7891a521d0?utm_campaign=maleskine&utm_content=note&utm_medium=seo_notes&utm_source=recommendation) 

### 6. 哪些还可以做得更好

敌人AI, 项目当中暂时没有实现AI功能. 后期有机会我将会补上. 

**参考:**

- [基于 Unity 引擎的游戏开发进阶之 敌人AI](https://zhuanlan.zhihu.com/p/29195825)
- [给猫看的游戏AI实战（二）视觉感知初步](https://zhuanlan.zhihu.com/p/28526310) 

### 7. 为什么叫做FFFFFlipping呢? 

纯属因为个人喜欢的两款放荡不羁的游戏

(1)[Eggggg（喷蛋狂人）](https://itunes.apple.com/cn/app/eggggg-%E5%96%B7%E8%9B%8B%E7%8B%82%E4%BA%BA-%E5%B9%B3%E5%8F%B0%E5%91%95%E5%90%90%E6%B8%B8%E6%88%8F/id1145738671?mt=8)

(2)[Flipping Legend (弹跳传奇)](https://itunes.apple.com/cn/app/flipping-legend/id1218046599?mt=8)

### 8. 个人博客

[CSDN: jingangxin36](https://blog.csdn.net/jingangxin666)

### 9. 项目地址

[jingangxin36/FFFFFlipping](https://github.com/jingangxin36/FFFFFlipping)


