const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8081 }); // WebSocketサーバーのポートを8081に設定

let phase = 0;

wss.on('connection', function connection(ws) {
  console.log('クライアントが接続しました');
  // 新規接続時に現在のフェーズを送信
  ws.send(JSON.stringify({ type: 'phase', value: phase }));

  ws.on('message', function incoming(message) {
    console.log('受信メッセージ:', message);
    const data = JSON.parse(message);

    if (data.type === 'changePhase') {
      phase = data.value;
      // すべてのクライアントにフェーズ変更をブロードキャスト
      wss.clients.forEach(function each(client) {
        if (client !== ws && client.readyState === WebSocket.OPEN) {
          client.send(JSON.stringify({ type: 'phase', value: phase }));
        }
      });
    }
  });

  ws.on('close', () => {
    console.log('クライアントが切断しました');
  });
});

console.log('WebSocketサーバーが ws://localhost:8081 で起動中');

