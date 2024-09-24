-- init.sql
CREATE USER IF NOT EXISTS 'newface'@'%' IDENTIFIED BY 'newface';
GRANT ALL PRIVILEGES ON NewFace.* TO 'newface'@'%';
FLUSH PRIVILEGES;