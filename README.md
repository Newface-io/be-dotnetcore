0. env 파일
    - 솔루션 레벨에 추가

1. AWS EC2 빌드
 
  1.1) 삭제
  sudo rm -rf be-dotnetcore
  
  1.2) git clone
  git clone https://github.com/Newface-io/be-dotnetcore.git
  
  1.3) 이동
  cd be-dotnetcore
  
  1.4) env 추가
  nano .env
  
  1.5) docker build
  docker-compose up --build


