# ベースイメージを指定
FROM nginx:latest

# 必要なパッケージをインストール
RUN apt-get update && apt-get install -y \
    brotli \
    && rm -rf /var/lib/apt/lists/*

# コンテンツのコピー
COPY ./webgl /usr/share/nginx/html
COPY ./nginx.conf /etc/nginx/nginx.conf

# ブラウザでアクセスするためのポートを公開
EXPOSE 80

# Nginxを起動
CMD ["nginx", "-g", "daemon off;"]
