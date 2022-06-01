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
mysql数据库不区分大小写,默认数据库为test

## sqlserver 
ALTER ROLE [test] ADD MEMBER [test]

CREATE LOGIN test WITH PASSWORD = 'pass@123';
CREATE USER test FOR LOGIN test;


CREATE SCHEMA test AUTHORIZATION test;

CREATE TABLE test.NineProngs (source int, cost int, partnumber int)

ALTER SERVER ROLE  [dbcreator]  ADD MEMBER [test];

CREATE USER test FOR LOGIN test;

select * from sys.sysusers s

drop schema test

drop user test
drop LOGIN test

CREATE SCHEMA test AUTHORIZATION test


Select sp.name as LoginName, sp.type_desc as LoginType,
dp.name as DBUser, dp.type_desc as UserType
from sys.server_principals sp
join sys.database_principals dp on dp.sid = sp.sid
where sp.name = 'test' -- your login

ALTER USER user_name
WITH LOGIN = new_login;

CREATE USER test

select * from sys.sysusers s 

alter USER test WITH DEFAULT_SCHEMA = test;

GRANT SELECT,INSERT ON SCHEMA :: test TO test;  

select * from sys.availability_groups

ALTER AVAILABILITY GROUP [AG-TEPF] REMOVE DATABASE test; 

select *from sys.dm_hadr_availability_group_states
