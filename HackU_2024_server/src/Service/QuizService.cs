using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Service;

public static class QuizService
{
    private static readonly (string Question, string[] Options, string Answer)[] Questions =
    [
        ("お正月に神社やお寺へ初詣に行く風習は、どの時代から一般的になったといわれているでしょうか？", new[] { "明治時代頃", "平安時代頃", "戦国時代頃", "鎌倉時代頃" }, "明治時代頃"),
        ("お正月料理として有名な「おせち料理」はもともと何を起源とした行事食でしょうか？", new[] { "五節句", "七夕", "秋祭り", "大晦日のお供え" }, "五節句"),
        ("お正月に門松を飾るのは、どのような神様をお迎えするためでしょうか？", new[] { "年神様", "雷神様", "稲荷神様", "八百万の神々" }, "年神様"),
        ("お正月に食べる「お雑煮」は地域によって味や具材が異なります。関東地方では一般的に何味が多いでしょうか？", new[] { "すまし仕立て", "味噌仕立て", "醤油ベースのとろみ仕立て", "カレー風味" }, "すまし仕立て"),
        ("お正月の行事で、新年初めて書く書道を何と呼ぶでしょうか？", new[] { "書初め", "初墨", "年始筆", "新春揮毫" }, "書初め"),
        ("お正月に子どもたちが大人からもらうお金のことを何と呼ぶでしょうか？", new[] { "お年玉", "初福銭", "賀銭", "新春贈与" }, "お年玉"),
        ("年賀状のやりとりが盛んになったのは、明治時代にあるサービスが普及したためです。そのサービスとは何でしょうか？", new[] { "郵便制度", "電話回線", "新聞配達", "電報配送" }, "郵便制度"),
        ("お正月前に大掃除をする風習は、どのような意味があるでしょうか？", new[] { "年神様を迎えるため家を清める", "冬至を祝うため", "農作物の害虫を追い払う", "新しい家具を購入する準備" }, "年神様を迎えるため家を清める"),
        ("お正月飾りの一つ「しめ縄」は、主にどこに飾るでしょうか？", new[] { "玄関先", "寝室", "仏壇の中", "台所" }, "玄関先"),
        ("大晦日に神社や寺で鳴らされる鐘のことを何と呼ぶでしょうか？", new[] { "除夜の鐘", "迎春の鐘", "初音の鐘", "年越し鐘" }, "除夜の鐘"),
        ("お正月に食べる麺料理で、一年の長寿や運を祈るものは何でしょうか？", new[] { "年越しそば", "初うどん", "新年ラーメン", "福そば" }, "年越しそば"),
        ("お正月に飾る「鏡餅」は、もちを何段にも重ねて置くことが多いでしょうか？", new[] { "2段", "3段", "4段", "5段" }, "2段"),
        ("お正月に行う「初詣」は、本来どのような行為を指すといわれるでしょうか？", new[] { "年が明けて初めて神社仏閣へ参拝すること", "初日の出を拝むこと", "新年最初に親戚に会うこと", "元日に初めて掃除をすること" }, "年が明けて初めて神社仏閣へ参拝すること"),
        ("お正月に見る夢、1月2日から3日にかけて見る夢を特に何と呼ぶでしょうか？", new[] { "初夢", "元夢", "新春夢", "賀夢" }, "初夢"),
        ("お正月に食べるおせち料理の重箱は、一般的に何段重ねでしょうか？", new[] { "四段重または五段重", "二段重", "三段重のみ", "六段重以上が定番" }, "四段重または五段重"),
        ("お正月に海や山などで拝む、1年で最初に昇る太陽を何と呼ぶでしょうか？", new[] { "初日の出", "元朝日", "新春光", "年明け陽" }, "初日の出"),
        ("お正月に遊ばれる伝統的な遊び「羽根つき」で使われる道具は何でしょうか？", new[] { "羽子板と羽根", "かるたと木札", "凧と糸", "独楽と紐" }, "羽子板と羽根"),
        ("お正月に遊ばれる「福笑い」は、目隠しをして何を組み合わせる遊びでしょうか？", new[] { "顔のパーツ", "文字の札", "色とりどりの布切れ", "動物の絵柄" }, "顔のパーツ"),
        ("お正月に、干支が描かれた年賀状が多く出されます。この干支は何年周期で回るでしょうか？", new[] { "12年", "10年", "5年", "20年" }, "12年"),
        ("お正月の三が日とは、一般的に何月何日までを指すでしょうか？", new[] { "1月3日まで", "1月5日まで", "1月7日まで", "1月10日まで" }, "1月3日まで"),
        ("お正月の七草粥を食べるのは一般的にいつでしょうか？", new[] { "1月7日", "1月2日", "1月15日", "1月20日" }, "1月7日"),
        ("お正月には福袋を買う人も多いです。福袋は通常何が入っているでしょうか？", new[] { "中身がわからない商品詰め合わせ", "現金のみ", "宝石のみ", "服飾品一種のみ" }, "中身がわからない商品詰め合わせ"),
        ("お正月に行われる新春行事で、皇居で行われる一般参賀は何と呼ばれるでしょうか？", new[] { "新年一般参賀", "元旦祝賀", "皇朝来訪", "初詣参内" }, "新年一般参賀"),
        ("お正月に行う書初めでは、特に縁起の良い言葉や年頭の目標を書きます。これを行うのは通常いつでしょうか？", new[] { "1月2日", "1月1日", "1月3日", "1月15日" }, "1月2日"),
        ("昔からお正月に行われる伝統的な遊びで、空高く上げるものは何でしょうか？", new[] { "凧", "羽子板", "独楽", "福笑い" }, "凧"),
        ("お正月に振る舞われるおせち料理は、重箱に詰められます。「重箱を重ねる」ことにはどのような願いが込められているでしょうか？", new[] { "めでたさを重ねる", "材料を増やすため", "捨てる手間を省くため", "高さを競うため" }, "めでたさを重ねる"),
        ("お正月飾りの一つ「門松」は、松ともう一つ主に何を組み合わせるでしょうか？", new[] { "竹", "梅", "杉", "桜" }, "竹"),
        ("お正月に贈られる「お年玉」の習慣は、元々は何を分け与える風習が起源とされるでしょうか？", new[] { "年神様への供え餅", "神社で買ったお守り", "仏壇の線香", "門松の松葉" }, "年神様への供え餅"),
        ("お正月料理のおせちの中で、黒豆はどのような願いを込めて食べられるでしょうか？", new[] { "まめまめしく勤勉に暮らす", "黒い色で魔除けをする", "甘い人生を願う", "富を呼ぶ" }, "まめまめしく勤勉に暮らす"),
        ("お正月料理のおせちの中で、数の子は何の象徴とされるでしょうか？", new[] { "子孫繁栄", "長寿祈願", "富貴繁栄", "学問成就" }, "子孫繁栄"),
        ("お正月料理のおせちの中で、栗きんとんの「きんとん」は何を象徴するとされるでしょうか？", new[] { "黄金（富）", "健康", "恋愛成就", "学業成就" }, "黄金（富）"),
        ("お正月には、初売りで「福袋」が販売されます。この福袋の元々の由来はどのようなものといわれるでしょうか？", new[] { "商売繁盛の縁起物として売られた", "観光客向けのお土産", "農民への年貢返礼", "神社が配る福引の外れ品" }, "商売繁盛の縁起物として売られた"),
        ("お正月に飲まれることがある祝い酒「屠蘇（とそ）」は、どのような薬草を用いたものとされるでしょうか？", new[] { "複数の生薬を漬け込んだ薬酒", "梅のみを使った酒", "桜花のみを使った酒", "稲穂を煎じた酒" }, "複数の生薬を漬け込んだ薬酒"),
        ("お正月に縁起物として飾る「橙（だいだい）」には、代々家が繁栄する願いが込められていますが、これはどこに飾ることが多いでしょうか？", new[] { "鏡餅の上", "門松の根元", "神棚の中", "玄関の床" }, "鏡餅の上"),
        ("お正月に食べる「伊達巻」は何を象徴するといわれるでしょうか？", new[] { "知識・学問（巻物）", "成功（巻き上げる）", "時間の流れ（渦巻）", "武士の礼儀（伊達者）" }, "知識・学問（巻物）"),
        ("新年に多くの人が訪れる有名な神社や寺で最も参拝者数が多いとされるのは、どのような場所でしょうか？", new[] { "明治神宮", "出雲大社", "伊勢神宮", "浅草寺" }, "明治神宮"),
        ("お正月に飾る「しめ飾り」には、主にどのような意味があるでしょうか？", new[] { "神聖な場所であることを示す結界", "音を鳴らして悪霊を追い払う", "火災を防ぐ", "風水的な幸運呼び" }, "神聖な場所であることを示す結界"),
        ("お正月に子供たちが外で遊ぶ伝統的な遊び「独楽（こま）回し」は、昔は何の象徴と考えられたでしょうか？", new[] { "円満な人生や安定", "時間を操る力", "敵を惑わす呪術", "豊作祈願" }, "円満な人生や安定"),
        ("お正月に食べられる「昆布巻き」は、昆布の語呂からどのような縁起を担いでいるでしょうか？", new[] { "「喜ぶ」から喜びをもたらす", "「込む」から人が集まる", "「昆」から子孫繁栄", "「布」から富を巻き込む" }, "「喜ぶ」から喜びをもたらす"),
        ("お正月には干支にちなんだ置物や飾りを飾ることがあります。これは主に何を象徴すると考えられているでしょうか？", new[] { "その年の守り神", "単なるデザイン要素", "商売繁盛の証", "過去の年の災厄封じ" }, "その年の守り神"),
        ("お正月の挨拶として有名な「明けましておめでとうございます」は、もともと何を祝う言葉でしょうか？", new[] { "新年を迎えたこと", "誕生日", "季節の変わり目", "満月の到来" }, "新年を迎えたこと"),
        ("お正月に演じられる伝統芸能「獅子舞」は、獅子が頭を噛むことで何をもたらすと信じられているでしょうか？", new[] { "厄払いと幸運", "知恵と学問", "商売繁盛のみ", "恋愛成就のみ" }, "厄払いと幸運"),
        ("お正月に貼る「門松」や「しめ飾り」などを片付ける日として一般的に知られる日はいつでしょうか？", new[] { "1月7日（松の内明け）", "1月3日", "1月15日（小正月）", "1月20日" }, "1月7日（松の内明け）"),
        ("お正月の風習である「年神様」は、どのようなものを司る神様といわれるでしょうか？", new[] { "農耕と祖先", "海と漁業", "風と雷", "山と木々" }, "農耕と祖先"),
        ("お正月に飾る鏡餅は、後にお汁粉やお雑煮などにして食べる風習があります。この行事を何と呼ぶでしょうか？", new[] { "鏡開き", "餅散らし", "餅祭り", "年餅供養" }, "鏡開き"),
        ("お正月に行われる行事で、書初めと並んで有名なものに「初釜」がありますが、初釜とは何を初めてする行為でしょうか？", new[] { "版画や印刷物を刷る", "着物を仕立てる", "釜で茶を点てる", "初めて竹を切る" }, "釜で茶を点てる"),
        ("お正月には甘酒や屠蘇など甘い酒が振る舞われることがありますが、これは主に何を願う行為でしょうか？", new[] { "健康長寿や厄除け", "商売繁盛", "学問上達", "芸術的才能の開花" }, "健康長寿や厄除け"),
        ("お正月に子供たちがもらうお年玉は、かつて家長が何を分け与えたものがルーツとされるでしょうか？", new[] { "歳神様に供えた丸餅", "新年の炭火", "野菜の種", "酒樽の残り" }, "歳神様に供えた丸餅"),
        ("「初日の出」を見るために人々が訪れるスポットはどのような場所が多いでしょうか？", new[] { "海岸や山頂", "街中の広場", "地下室", "城の跡地" }, "海岸や山頂"),
        ("お正月を象徴する花として生けられることの多い植物は次のうちどれでしょうか？", new[] { "松・竹・梅", "桜・藤・菖蒲", "菊・牡丹・蓮", "藤・朝顔・向日葵" }, "松・竹・梅")
    ];


    public static ServerMessage[] Quiz(string thisTurnPlayerId)
    {
        return Questions.Select(questionData => new ServerMessage
        {
            QuizStart = new QuizStart
            {
                Type = "QUIZ_START",
                Data = new QuizStart.Types.Data
                {
                    PlayerId = thisTurnPlayerId, QuizQuestion = questionData.Question,
                    Options = { questionData.Options },
                    Answer = questionData.Answer
                }
            }
        }).ToArray();
    }
    
    public static async UniTask<ServerMessage[]?> OnQuizAnswerAsync(Client client, QuizAnswer req)
    {
        Console.WriteLine("QuizAnswer");
        return await UniTask.Run<ServerMessage[]?>(() =>
        {
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return null;
            if (room.State != Room.RoomState.Gaming)
                return null;
            if (room.UserIsAnswered[client.UserID])
                return null;
            room.UserIsAnsweredOrder[client.UserID] = room.UserIsAnswered.Count(kv => kv.Value);
            room.UserIsAnswered[client.UserID] = true;
            room.UserAnswer[client.UserID] = req.Data.Answer;
            DataBaseManager.UpdateRoomData(room);
            return null;
        });
    }
    
    public static string[]? CheckCorrect(string roomName)
    {
        // return correct User IDs
        var room = DataBaseManager.GetRoom(roomName);
        if (room is null)
            return null;
        if (room.State != Room.RoomState.Gaming)
            return null;
        if (!room.UserIsAnswered.Values.All(isAnswered => isAnswered)) return null;
        var correctUserIds = room.UserAnswer
            .Where(kv => kv.Value == room.QuizData!.Data.Answer)
            .Select(kv => kv.Key).ToArray();
        // sort by Answered Order
        correctUserIds = correctUserIds.OrderBy(id => room.UserIsAnsweredOrder[id]).ToArray();
        return correctUserIds;
    }
    public static ServerMessage[]? QuizResult(string roomName, string[] correctUserIds)
    {
        var room = DataBaseManager.GetRoom(roomName);
        if (room is null)
            return null;
        if (room.State != Room.RoomState.Gaming)
            return null;
        Console.WriteLine("QuizResult");
        var res = new ServerMessage[room.UserIDs.Count];

        var correctAnsweredOrder = room.UserIsAnsweredOrder
            .Where(kv => correctUserIds.Contains(kv.Key)).ToDictionary();
        correctAnsweredOrder = correctAnsweredOrder.OrderBy(kv => kv.Value).ToDictionary();
        var correctAnsweredOrderValues = correctAnsweredOrder.Values.ToArray();
        for (var i = 0; i < correctAnsweredOrderValues.Length; i++)
        {
            correctAnsweredOrder[correctAnsweredOrder.First(kv => kv.Value == correctAnsweredOrderValues[i]).Key] = i;
        }
        
        for (var i = 0; i < room.UserIDs.Count; i++)
        {
            var userId = room.UserIDs[i];
            var isCorrect = correctUserIds.Contains(userId);
            var otoshidama = 0;
            if (correctAnsweredOrder.TryGetValue(userId, out var value))
            {
                if (value != null) otoshidama = 1000 * Math.Max(5 - value.Value, 1);
            }
            
            res[i] = new ServerMessage
            {
                QuizResult = new QuizResult
                {
                    Type = "QUIZ_RESULT",
                    Data = new QuizResult.Types.Data
                    {
                        PlayerId = userId,
                        IsCorrect = isCorrect,
                        OtoshidamaAmount = otoshidama
                    }
                }
            };
        }

        return res;
    }
}