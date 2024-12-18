﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var beforePos = 0;//スクロールの値の比較用の設定

//スクロール途中でヘッダーが消え、上にスクロールすると復活する設定を関数にまとめる
function ScrollAnime() {
    var elemTop = $('#main').offset().top;//#area-2の位置まできたら
    var scroll = $(window).scrollTop();
    //ヘッダーの出し入れをする
    if (scroll == beforePos) {
        //IE11対策で処理を入れない
    } else if (elemTop > scroll || 0 > scroll - beforePos) {
        //ヘッダーが上から出現する
        $('#header').removeClass('UpMove'); //#headerにUpMoveというクラス名を除き
        $('#header').addClass('DownMove');//#headerにDownMoveのクラス名を追加
    } else {
        //ヘッダーが上に消える
        $('#header').removeClass('DownMove');//#headerにDownMoveというクラス名を除き
        $('#header').addClass('UpMove');//#headerにUpMoveのクラス名を追加
    }

    beforePos = scroll;//現在のスクロール値を比較用のbeforePosに格納
}


// 画面をスクロールをしたら動かしたい場合の記述
$(window).scroll(function () {
    ScrollAnime();//スクロール途中でヘッダーが消え、上にスクロールすると復活する関数を呼ぶ
});

// ページが読み込まれたらすぐに動かしたい場合の記述
$(window).on('load', function () {
    ScrollAnime();//スクロール途中でヘッダーが消え、上にスクロールすると復活する関数を呼ぶ
});

function goBackOrHome() {
    if (window.history.length > 1) {
        window.history.back();
    } else {
        window.location.href = 'https://mintsports.net/Home/Index'; // ハードコードされたURL
    }
}
