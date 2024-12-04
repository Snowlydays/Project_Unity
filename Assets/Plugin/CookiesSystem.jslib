mergeInto(LibraryManager.library, {
    //cookieに値を設定するメソッド
    SetCookieJS: function(key,value) {
        var keystr=UTF8ToString(key)//string引数を正常にさせるメソッド(これを介さないとめちゃくちゃな数値データになる)
        document.cookie = keystr +"="+ value + "; max-age=31536000"//"key=value"でcookieを設定できる
    },
    //cookieから特定の値を取得するメソッド
    GetCookieValueJS: function(key) {
        var keystr=UTF8ToString(key)
        var cookiesArray = document.cookie.split(';');
        for (var i = 0; i < cookiesArray.length; i++)
        {
            var cArray = cookiesArray[i].split('=');
            if (cArray[0].trim() === keystr)
            {
                var bufferSize = lengthBytesUTF8(cArray[1].trim()) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(cArray[1].trim(), buffer, bufferSize);
                return buffer;
            }
        }
        return '';
    }
});