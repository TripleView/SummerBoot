# 开始
复制app.json.default为app.json文件，然后打开，修改里面的oracle，sqlserver和mysql的数据库链接，添加nacos的服务器地址，即可开启单元测试。
# 设置
## oracle
oracle数据库不区分大小写，oracle需要有2个用户，test和test1，连接账户为test，需要有对test1的访问权限，如提示 对表空间"USERS"无权限，请执行
````sql
alter user TEST  quota unlimited on users;
alter user TEST1 quota unlimited on users;
````
## mysql
mysql数据库不区分大小写
