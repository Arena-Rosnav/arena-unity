using System.Linq;

namespace PNM
{
    public class MagicNumber : Enumeration<MagicNumber>
    {
        public readonly string Extension;
        public bool IsBinary
        {
            get => Id % 2 == 1;
        }
        public bool IsAscii
        {
            get => !IsBinary;
        }

        public static readonly MagicNumber P1 = new(nameof(P1),"Portable Bitmap", 1, "pbm");
        public static readonly MagicNumber P2 = new(nameof(P2),"Portable Graymap", 2, "pgm");
        public static readonly MagicNumber P3 = new(nameof(P3),"Portable Pixmap", 3, "ppm");
        public static readonly MagicNumber P4 = new(nameof(P4),"Portable Bitmap", 4, "pbm");
        public static readonly MagicNumber P5 = new(nameof(P5),"Portable Graymap", 5, "pgm");
        public static readonly MagicNumber P6 = new(nameof(P6),"Portable Pixmap", 6, "ppm");
        public readonly string DisplayName;

        private MagicNumber(string name, string displayName, int id, string extension) : base(id, name)
        {
            Extension = extension;
            DisplayName = displayName;
        }

        public static MagicNumber FromName(string name){
            if(name.Length > 2) name = name.Substring(0,2);
            return MagicNumber.GetAll().FirstOrDefault(m => m.Name == name);
        }

        public static MagicNumber FromId(int id){
            var m = MagicNumber.GetAll().FirstOrDefault(m => m.Id == id);
            return m;
        }

        public static bool TryFromName(string name, out MagicNumber magicNumber){
            magicNumber = FromName(name);
            if(magicNumber == null) return false;
            return true;
        }

        public static bool TryFromId(int id, out MagicNumber magicNumber){
            magicNumber = FromId(id);
            if(magicNumber == null) return false;
            return true;
        }

        public override string ToString()
        {
            return "P" + Id;
        }
    }

}
