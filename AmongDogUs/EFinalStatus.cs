namespace AmongDogUs;

internal enum EFinalStatus : int
{
    Alive, // 生存
    Sabotage, // サボ
    Reactor, // リアクターサボ
    Exiled, // 追放
    Misfire, // 誤爆
    Suicide, // 自殺
    Dead, // 死亡(キル)
    LackO2, // 酸素不足
    Bomb, // 爆発
    Torched, // 焼殺
    Revival, // 復活
    Disconnected // 切断
}
