﻿using System;
using System.Diagnostics;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 駒を表す定数
    /// </summary>
    /// <remarks>
    /// PAWN   : 歩
    /// LANCE  : 香
    /// KNIGHT : 桂
    /// SILVER : 銀
    /// BISHOP : 角
    /// ROOK   : 飛
    /// GOLD   : 金
    /// KING   : 王
    /// PRO_PAWN   : 成歩(と)
    /// PRO_LANCE  : 成香
    /// PRO_KNIGHT : 成桂
    /// PRO_SILVER : 成銀
    /// HORSE      : 馬
    /// DRAGON     : 龍
    /// QUEEN      : 成金(この駒は無いのでQUEENを当ててある)
    /// </remarks>
    public enum Piece : Int32
    {
        NO_PIECE, PAWN, LANCE, KNIGHT, SILVER, BISHOP, ROOK, GOLD,
        KING, PRO_PAWN, PRO_LANCE, PRO_KNIGHT, PRO_SILVER, HORSE, DRAGON, QUEEN,

        PROMOTE = 8, // 成りを表す
        WHITE = 16,  // 後手を表す

        ZERO = 0,    // Pieceの開始番号
        NB = 32,     // Pieceの終端を表す 

        HAND_NB = KING,   // 手駒になる駒種の最大+1
    };

    /// <summary>
    /// Pieceに関するextension methodsを書いておくクラス
    /// </summary>
    public static class PieceExtensions
    {
        /// <summary>
        /// 値が正常な範囲であるかを判定する。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static bool IsOk(this Piece piece)
        {
            return Piece.ZERO <= piece && piece < Piece.NB;
        }

        // "□"(四角)は文字フォントによっては半分の幅しかない。"口"(くち)にする。
        private static readonly string[] PIECE_KANJI = {
                " 口"," 歩"," 香"," 桂"," 銀"," 角"," 飛"," 金"," 玉"," と"," 杏"," 圭"," 全"," 馬"," 龍"," 菌"," 王",
                      "^歩","^香","^桂","^銀","^角","^飛","^金","^玉","^と","^杏","^圭","^全","^馬","^龍","^菌","^王" };

        /// <summary>
        /// 日本語の文字列にする。
        /// 盤面表示用なので2文字から成る。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string Pretty(this Piece piece)
        {
            return PIECE_KANJI[piece.ToInt()];
        }

        /// <summary>
        /// 手駒などを表示する用なのでpretty()とは異なり、漢字1文字で出力する。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static char Pretty2(this Piece piece)
        {
            return PIECE_KANJI[piece.ToInt()][1];
        }

        private const string USI_PIECE = ". P L N S B R G K +P+L+N+S+B+R+G+.p l n s b r g k +p+l+n+s+b+r+g+k";

        /// <summary>
        /// USI文字列に変換する。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string ToUsi(this Piece piece)
        {
            if (!piece.IsOk())
                return "??";

            var p = (int)piece.ToInt();

            // 末尾の人力trim
            var length = (USI_PIECE[p * 2 + 1] == ' ') ? 1 : 2;
            return USI_PIECE.Substring(p * 2, length);
        }

        /// <summary>
        /// pが先手の駒であるか、後手の駒であるかを返す。
        /// p==EMPTYの場合、先手の駒扱いをする。
        /// </summary>
        public static Color PieceColor(this Piece piece)
        {
            return (piece < Piece.WHITE) ? Color.BLACK : Color.WHITE;
        }

        /// <summary>
        /// 後手の歩→先手の歩のように、後手という属性を取り払った駒種を返す
        /// </summary>
        public static Piece PieceType(this Piece piece)
        {
            return ((Piece)((Int32)piece & ~(Int32)Piece.WHITE));
        }

        /// <summary>
        /// 成ってない駒を返す。後手という属性も消去する。
        /// 例) 成銀→銀 , 後手の馬→先手の角
        /// ただし、pc == KINGでの呼び出しはNO_PIECEが返るものとする。
        /// </summary>
        public static Piece RawPieceType(this Piece piece)
        {
            return (Piece)(piece.ToInt() & 7);
        }

        /// <summary>
        /// 成り駒であるかどうかを判定する
        /// </summary>
        public static bool IsPromote(this Piece piece)
        {
            return (piece.ToInt() & Piece.PROMOTE.ToInt()) != 0;
        }

        /// <summary>
        /// pieceをInt32の値で取り出したいときに用いる。
        /// </summary>
        /// <returns></returns>
        public static Int32 ToInt(this Piece piece)
        {
            return (Int32)piece;
        }
    }

    /// <summary>
    /// Model.Shogi用のヘルパークラス
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// pcとして先手の駒を渡し、cが後手なら後手の駒を返す。cが先手なら先手の駒のまま。
        /// pcとしてNO_PIECEは渡してはならない。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Piece MakePiece(Color c, Piece pc)
        {
            Debug.Assert(pc.PieceColor() == Color.BLACK && pc != Piece.NO_PIECE);
            return (Piece)((c.ToInt() << 4) + pc.ToInt());
        }

        /// <summary>
        /// 成り駒を返す。
        /// pcとして先手の駒を渡し、cが後手なら後手の駒(の成り駒)を返す。cが先手なら先手の駒(の成り駒)のまま。
        /// pcとしてNO_PIECEは渡してはならない。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Piece MakePiecePromote(Color c, Piece pc)
        {
            Debug.Assert(pc.PieceColor() == Color.BLACK && pc != Piece.NO_PIECE && !pc.IsPromote());
            return (Piece)((c.ToInt() << 4) + pc.ToInt() + Piece.PROMOTE.ToInt());
        }

        /// <summary>
        /// USIの駒文字列(1バイト文字列)
        /// </summary>
        public static readonly string USI_MAIN_PIECE = "PLNSBRGK";

        /// <summary>
        /// USI文字列の1バイト駒をPieceに変換する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Piece FromUsiPiece(char c)
        {
            var c2 = char.ToUpper(c);

            for (int i = 0; i < 8; ++i)
                if (USI_MAIN_PIECE[i] == c2)
                    return (Piece)(Piece.PAWN.ToInt() + i + ((c==c2) ? 0 : Piece.WHITE.ToInt()) );

            // 見つからず
            return Piece.NO_PIECE;
        }

    }

}
