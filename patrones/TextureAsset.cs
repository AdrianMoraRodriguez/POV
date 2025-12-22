using LearnOpenTK.Common; //Texture

public class TextureAsset : Asset
{
	public Texture tex {get; private set;}
	public TextureAsset(Texture tex) : base(Asset.Type.Texture)
	{
		this.tex=tex;
	}

}


