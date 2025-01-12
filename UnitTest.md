# test nacos login
docker run --name nacos-standalone-auth -e MODE=standalone -e NACOS_AUTH_ENABLE=true -e NACOS_AUTH_TOKEN=bmFjb3NfMjAyNDAxMTBfc2hpZ3poX25hY29zX3Rva2Vu -e NACOS_AUTH_IDENTITY_KEY=abc -e NACOS_AUTH_IDENTITY_VALUE=abc -p 8848:8848 -d -p 9848:9848  nacos/nacos-server:latest

# test nacos no login
docker run --name nacos-standalone -e MODE=standalone   -p 8848:8847 -d -p 9848:9847  nacos/nacos-server:latest
