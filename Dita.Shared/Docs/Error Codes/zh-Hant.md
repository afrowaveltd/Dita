# 错误代码

Dita使用一个**距离分出,统一的出错代码架构**,既提供域特有enum又提供单一的call-all类型. 系统中的每一个错误——从网络失败到磁盘 I/O,从认证到配置——都由这个等级的成员来代表.

## 建筑

### 范围分配

范围
|-------|----------|----------|
1,000 - 1999 (单位:千美元)
2000 -- 2999年
3000-3999 (韩语)
4000-4999 (中文(简体) )
5000-5999 (中文(简体) )
60000-6999 (中文(简体) )
700 000-7999 (中文(简体) )
8000-8999 (中文(简体) )
9000-9999 (中文(简体) )

### 二元图案

每个出错域分别由****一个有焦点的子enum(例如)和统一enum中的条目来代表. 子名使用赤色名称; 统一的enum前缀名称包含该类别:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

这允许代码在得知上下文时与特定域类型相配合,同时也支持在所有域中工作的通用错误处理.

### 发送器

每个子enum定义其范围的基础值(如.). 方法确认这一点并返回.

## 错误代码类

Enum将所有子enum值合并为一个有**非重叠**整数范围的单一类型. 伴生静态类提供人性化:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### 人性化逻辑

采取公约重叠办法:

1. PascalCase 名称通过 regex 分割成单词
2. 已知的缩写规范化(Io-O、Api-API、Dns-DNS、Http-HttP、Ssl-SSL、Mfa-MFA、OAuth-OAuth、Sso-SSO、Xml-XML、Json-JSON、Url-URL)
3. 全盖符( 如) 保存
4. 以返回结束的值

## 域特有词

### 网络错误(1 000-1999)

覆盖DNS,SSL/TLS,代理,网关,HTTP协议出错,连接,并请求存在生命周期问题.

知名成员
|---|---|
1 000个
1001 (英语)
1002 (韩语)
1003 (英语)
1004 (英语)
1005 (英语)
1006号
1007 (英语)
1008 (英语)
1009 (英语)
1010号
1019 (英语)
1020号
1021号

### 存储错误(2000-2999)

包括数据库连接、交易(承诺/回滚/超时)、完整性(约束、僵局、外国密钥)、计划管理、备份/备份、复制和配额.

知名成员
|---|---|
联合国
联合国
联合国
页:1
2010年统计
2012 (中文(简体) )
2013 (中文(简体) )
2018 (英语)
2023 (英语)
2029 (英语)

### 磁盘错误( 3000– 3999)

覆盖低等物理磁盘和驱动器出错:坏扇区,SMART故障,RAID退化,分区表,硬件故障,挂载/卸载,格式,和弹出操作.

知名成员
|---|---|
3000个
3001 (英语)
3010号
3012号
3021 (英语)
3027 (英语)
3032 (英语)

### 文件系统错误( 4000–4999)

覆盖文件系统操作错误:访问/permission,文件锁定,压缩/解压缩/加密,路径问题,符号链接,共享违规,以及普通I/O操作.

知名成员
|---|---|
4000个
4001 (英语)
第4013号
4011号
4023 (中文(简体) )
第4024号
4028 (中文(简体) )

### 本地化 错误 (5000–5999)

覆盖本地化管道特有的出错:词典,编码,地语验证,复数形式,外部翻译API(自动,可用性,队列,超时),和字符串格式化.

知名成员
|---|---|
5000块
5001 (英语)
5007 (英语)
第5014号
第5015号
第5016号
5018 (简体中文)

### 认证错误( 6000–6999)

包括认证和授权:证书、令牌(更新/访问)、会话、MFA/2FA、生物鉴别、证书、OAuth、SSO和账户状态(失效、过期、锁定).

知名成员
|---|---|
6000个
6001 (英语)
6004 (英语)
第6015号
第6024号
6026 (英语)

### 验证错误( 7000-7999)

覆盖输入验证:格式检查(电子邮件,电话,URL,JSON,XML,日期时间),范围/长度限制,转换失败,需要的字段,模式/regex,以及密码复杂.

知名成员
|---|---|
7 000个
7003 (中文(简体) )
7016号
7018 (英语)

### 配置错误( 8000–8999)

封面配置和设置:文件访问,解析,验证,密钥/密钥金库,连接字符串,DI,特征标记,环境变量,以及schema/版本不匹配.

知名成员
|---|---|
8000个
8001 (简体中文)
8016 (英语)
8019号

### 将军(900-9999)

抓取全应用程序错误:内存,货币,许可,限速,线程,资源管理,特性支持,以及未处理的例外.

知名成员
|---|---|
9000个
9004号
9007号
9015 (英语)
9014号

## 输油管

### 进程Stage

定义自动翻译管道的相继阶段:

数值
|-------|------|-------------|
0 个
1个
2个
3个
页:1
页:1

### 本地化MessageType

管道发出的实时信息:

数值
|-------|------|---------|
0 个
1个
2个
3个
页:1
页:1
6个

### 翻译 目标

指定要翻译的内容类型 :

数值
|-------|------|---------------|
0 个
1个
2个

### 换词

本地化字典条目的 CRUD 类似更改状态 :

数值
|-------|------|
0 个
1个
2个
3个

### 比较

用于评价/过滤值的比较运算符:

数值
|-------|------|----------|
0 个
1个
2个
3个
页:1
页:1
6个

### 性别

地方化的语法/社会性别:

数值
|-------|------|
0 个
1个
2个
3个

## 使用错误代码

### 编审中的报告

记录中包含翻译错误:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### 在API的答复中

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### 使任何代码人性化

```csharp
// From enum value
string text = ErrorCodeText.ErrorText(ErrorCode.StorageDeadlockDetected);
// → "Storage deadlock detected"

// From raw integer (validates against defined values)
string text2 = ErrorCodeText.ErrorText(2010);
// → "Storage deadlock detected"

// Undefined code
string text3 = ErrorCodeText.ErrorText(99999);
// → "Unknown error (99999)"
```
