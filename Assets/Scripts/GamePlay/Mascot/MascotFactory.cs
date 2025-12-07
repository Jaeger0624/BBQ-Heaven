public interface IMascotFactory{
    Mascot GenerateMascot();
}

public class MascotFactory_指定系列 : IMascotFactory
{
    private string series;
    public MascotFactory_指定系列(string series)
    {
        this.series = series;
    }
    public Mascot GenerateMascot()
    {
        
        throw new System.NotImplementedException();
    }
}