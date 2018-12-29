

# QEnc

QEnc is a video compression and package tool based on Drag-and-drop operations

It use FFMPEG as decoder/encoder and allow any formats of Video, Audio or Subtitle file as import. Generate Mp4/M4a file with H.264/AVC video stream and AAC audio stream as export.

## Features

- Simple UI

- Easy to use

- Smooth animation

- Use Drag-and-drop to import files
- Supported Language: English, Simplified Chinese 

![screenshot](screenshots\screenshot.png)

## Quick Start

Drag the file to the Video/Audio/Subtitle config area to import the file. The software will automatically check the file with a blue circle shows behind the Video/Audio/Subtitle label, witch mean processing. After checking, a red cross or a green tick will replace the blue circle, which implies import succeed or failed. If it is succeed and the config of coding are correct, click the Start button to start coding. The final output will appear in the same folder of your input file.

Hint: Drag the file to the Left side can import is Video, Audio and Subtitle at the same time.

##About Status Icon

- Green tick = Loaded and Ready to encode
- Orange horizontal line = Loaded and Do not encode, just copy the stream
- Red cross = Failed to import
- Blue circle = Processing

Right click to remove the Loaded file

Left click to switch the processing mode between use encoder and copy stream

## About config

The first line of both Video config and Audio config is the quality. The second line of the Video config is the additional parameters pass to the X264 encoder. (Use the format "[Name1]=[Value1]:[Name2]=[Value2]...") However, the second line of Audio config is reserved.

##About queue

Click *Multi* button to expand Queue panel (It also affect the process mode when you click the *Start* button). Click *Queue* button to add current config into the queue. Click *Not queue* button to remove.

Attention: Once you add a config to the queue you can not change it any more.



# QEnc - 简体中文

QEnc是一种基于拖放操作的视频压制和封装工具

它使用FFMPEG作为解码器/编码器，允许任何格式的视频，音频或字幕文件作为导入。 生成具有H.264 / AVC视频流和AAC音频流的Mp4 / M4a文件作为导出。

## 特性

 - 简单的用户界面
- 使用方便
 - 流畅的动画
 - 使用拖放功能导入文件
 - 支持的语言：英语，简体中文

![screenshot](screenshots\screenshot_zh-CN.png)

##快速开始

将文件拖到视频/音频/字幕配置区域以导入文件。 该软件将自动检查文件，在视频/音频/字幕标签后面显示一个蓝色圆圈，表示处理中。 检查后，红色十字或绿色勾号将替换蓝色圆圈，这意味着导入成功或失败。 如果成功并且编码配置正确，请单击“开始”按钮开始编码。 最终输出文件将出现在输入文件的同一文件夹中。

提示：将文件拖到左侧可以同时导入视频，音频和字幕。

## 关于状态图标

 - 绿色勾选 = 加载并准备编码
 - 橙色横线 = 已加载且不进行编码，只需复制流即可
 - 红色十字 = 导入失败
 - 蓝色圆圈 = 处理中

右键单击以删除已加载的文件

左键单击以在使用编码器和复制流之间切换处理模式

## 关于配置

视频配置和音频配置的第一行是质量。 视频配置的第二行是传递给X264编码器的附加参数。 （使用格式"[Name1]=[Value1]:[Name2]=[Value2]..."）然而，音频配置的第二行是预留的。

## 关于队列

单击“多个”按钮以展开“队列”面板（单击“开始”按钮时，它也会影响过程模式）。 单击“队列”按钮将当前配置添加到队列中。 单击“未队列”按钮以删除。

注意：将配置添加到队列后，您无法再进行更改。