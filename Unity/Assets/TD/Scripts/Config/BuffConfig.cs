namespace TD
{
    public class BuffConfig
    {
        public int Id;
        //作用类型
        public int Type;
        //作用值
        public int Value;
        //作用间隔 每间隔Interval产生Value效果 0:一次性效果
        public int Interval;
        //生命值 ms
        public int Life;
    }
}