public class Asset {

	public enum Type {

		Generic,
		Mesh,
		Texture

	}

	 public Type type {get; private set; } = Type.Generic;

	 public Asset()
	{

	}

	public Asset(Type type)
	{
		this.type=type;
	}


}


