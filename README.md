# GameFramework.FairyGUI

基于EllanJiang / GameFramework框架UI接口的设计，与强大的开源UI引擎FairyGUI实现的UI模块，为GameFramework的使用者与爱好者提供与UGUI不同的开发体验。

项目对GameFramework源码无调整、无要求，可自行选用任意版本或魔改版本的GameFramework。

## Road Map

- 基础功能

    - 与GameFramework原UGUI版本接口完全符合的IUIFormLogic界面逻辑接口，降低从UGUI转至FairyGUI上手难度

    - 完全符合GameFramework中UI框架实现的`FairyGUIFormHelper`和`FairyGUIGroupHelper`实现，可自由添加和移除本模块，不会侵入原有框架

- UIPackage管理

    - 将通过`FairyGUIComponent`来统一管理游戏运行期间所需的UIPackage实例

- 代码生成器支持

    - 使用C#实现FairyGUI的代码生成器，与FairyGUI原生代码生成功能对比，更为方便开发者根据自己的需求进行调整代码模板与增减功能

    - 支持UIForm与UIComponent两种类型的代码生成，摆脱手动获取组件的繁琐流程

    - 支持运行时自动绑定生成代码，无需手动设置，减少无用繁琐的工作量

