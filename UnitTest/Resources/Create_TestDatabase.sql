
if exists(select 1 from sys.databases where name='Sales')
drop database Sales

create database Sales