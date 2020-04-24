# SummerBoot
> 将SpringBoot的先进理念与C#的简洁优雅合二为一，致力于让net core开发变得更简单。

## Getting Started
### Nuget
 你可以运行以下命令在你的项目中安装 SummerBoot。
 
 ```PM> Install-Package SummerBoot```
## 支持框架
net core 3.1

# 如何使用
参考example项目

## 框架中的一些接口
* IInitializing。如果一个类实现了该接口，那么在生成动态代理类,并执行属性注入后,从IOC容器中返回给调用方前，将会执行方法<b>AfterPropertiesSet</b>，可在该方法中执行类的初始化操作。

* IInterceptor。拦截器接口，实现该接口，可自定义拦截器，如有需要，可在注册服务的时候添加拦截器。

* ISerialization。框架中默认的序列化反序列化接口，可替换，默认为json。

* IUnitOfWork。工作单元接口，熟悉工作单元的，可重写该接口，并注入到框架中。

* ICacheManager。缓存管理器接口，可重写该接口实现各种有意思的缓存操作，比如多级缓存。

* IKeyGenerator。缓存键值生成器接口，可重写该接口，实现自定义key生成规则。

* IRedisCacheWriter。实际操作redis的接口，summerBootCache部分的底层核心，可直接属性注入使用。

* IPatternMatcher。校验字符串是否符合某规则的接口。

* IRepository。自定义仓储接口，框架默认底层实现基于dapper，可重写替换。

* IDbFactory。生成IDbConnection的工厂接口，可重写替换。