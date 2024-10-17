# ベースイメージを指定
FROM node:18-alpine

# 作業ディレクトリの設定
WORKDIR /app

# パッケージのコピーとインストール
COPY package.json .
COPY package-lock.json .
RUN npm install

# サーバースクリプトのコピー
COPY server.js .

# サーバーのポートを公開
EXPOSE 8081

# サーバーの起動
CMD ["node", "server.js"]

