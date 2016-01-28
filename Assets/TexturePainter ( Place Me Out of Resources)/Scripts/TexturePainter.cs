/// <summary>
/// CodeArtist.mx 2015
/// This is the main class of the project, its in charge of raycasting to a model and place brush prefabs infront of the canvas camera.
/// If you are interested in saving the painted texture you can use the method at the end and should save it to a file.
/// </summary>


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Painter_BrushMode{PAINT,DECAL, ERASE};
public class TexturePainter : MonoBehaviour {
	public GameObject brushCursor,brushContainer; //The cursor that overlaps the model and our container for the brushes painted
	public Camera sceneCamera,canvasCam;  //The camera that looks at the model, and the camera that looks at the canvas.
	public Sprite cursorPaint,cursorDecal; // Cursor for the differen functions 
	public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
	public Material baseMaterial; // The material of our base texture (Were we will save the painted texture)



	Painter_BrushMode mode; //Our painter mode (Paint brushes or decals)
	float brushSize=1.0f; //The size of our brush
	Color brushColor; //The selected color
	int brushCounter=0,MAX_BRUSH_COUNT=1000; //To avoid having millions of brushes
	bool saving=false; //Flag to check if we are saving the texture


	[SerializeField]
	private STM_PaintCamera paintCam;
	[SerializeField]
	private GameObject canvasBaseMaterial;
	
	private GameObject brushObj;

	void Update () {
		//brushColor = ColorSelector.GetColor ();	//Updates our painted color with the selected color

		if (Application.isEditor) {
			if (Input.GetMouseButton (0)) {
				if(!paintCam._isOnRotate)
					DoAction ();
			}
		} else {
			if (Input.touchCount == 1) {
				if(!paintCam._isOnRotate)
					DoAction ();
			}
		}


		UpdateBrushCursor ();
	}

	//The main action, instantiates a brush or decal entity at the clicked position on the UV map
	void DoAction(){	
		if (saving)
			return;
		Vector3 uvWorldPosition=Vector3.zero;		
		if(HitTestUVPosition(ref uvWorldPosition)){

			switch(mode){
				case Painter_BrushMode.PAINT:
					brushObj=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
					brushObj.GetComponent<SpriteRenderer>().color=brushColor; //Set the brush color
					brushObj.GetComponent<SpriteRenderer>().sprite = cursorPaint;
					break;
				case Painter_BrushMode.DECAL:
					brushObj=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/DecalEntity")); //Paint a decal
					brushObj.GetComponent<SpriteRenderer>().sprite = cursorDecal;
					break;
				case Painter_BrushMode.ERASE:
					brushObj=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
					Color tempColor = brushColor;
					brushObj.GetComponent<SpriteRenderer>().color= tempColor; //Set the brush color
					brushObj.GetComponent<SpriteRenderer>().sprite = cursorPaint;

					break;
			}
		
			brushColor.a=brushSize*2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.parent=brushContainer.transform; //Add the brush to our container to be wiped later
			brushObj.transform.localPosition=uvWorldPosition; //The position of the brush (in the UVMap)
			brushObj.transform.localScale=Vector3.one*brushSize;//The size of the brush
		}
		brushCounter++; //Add to the max brushes
		if (brushCounter >= MAX_BRUSH_COUNT) { //If we reach the max brushes available, flatten the texture and clear the brushes
			brushCursor.SetActive (false);
			saving=true;
			Invoke("SaveTexture",0.1f);
			
		}
	}
	//To update at realtime the painting cursor on the mesh
	void UpdateBrushCursor(){
		Vector3 uvWorldPosition=Vector3.zero;
		if (HitTestUVPosition (ref uvWorldPosition) && !saving) {
			brushCursor.SetActive(true);
			brushCursor.transform.position =uvWorldPosition+brushContainer.transform.position;									
		} else {
			brushCursor.SetActive(false);
		}		
	}
	//Returns the position on the texuremap according to a hit in the mesh collider
	bool HitTestUVPosition(ref Vector3 uvWorldPosition){
		RaycastHit hit;
		Vector3 cursorPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay=sceneCamera.ScreenPointToRay (cursorPos);
		if (Physics.Raycast(cursorRay,out hit,200)){
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;			
			Vector2 pixelUV  = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			uvWorldPosition.x=pixelUV.x-canvasCam.orthographicSize;//To center the UV on X
			uvWorldPosition.y=pixelUV.y-canvasCam.orthographicSize;//To center the UV on Y
			uvWorldPosition.z=0.0f;
			return true;
		}
		else{		
			return false;
		}
		
	}
	//Sets the base material with a our canvas texture, then removes all our brushes
	void SaveTexture(){		
		brushCounter=0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = canvasTexture;
		Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);		
		tex.ReadPixels (new Rect (0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
		tex.Apply (); 
	//	Texture2D tempBGText = canvasBaseMaterial.GetComponent<Renderer> ().materials [0].GetTexture("_MainTex") as Texture2D;
		Texture2D combined = tex;
		if((Texture2D) canvasBaseMaterial.GetComponent<Renderer> ().materials [0].mainTexture)
			combined = CombineTextures ((Texture2D) canvasBaseMaterial.GetComponent<Renderer> ().materials [0].mainTexture,tex);
		RenderTexture.active = null;

		//Isntead of saving directly to material in directory project, save it on object as instance.. before saving texture as PNG
	//	baseMaterial.mainTexture = tex;	//Put the painted texture as the bases
		canvasBaseMaterial.GetComponent<Renderer> ().materials [0].mainTexture = combined;

		foreach (Transform child in brushContainer.transform) {//Clear brushes
			Destroy(child.gameObject);
		}
		//StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
		Invoke ("ShowCursor", 0.1f);
	}


	Texture2D CombineBackGroundAndPainted(Texture2D background, Texture2D watermark)
	{
		Texture2D tempTex = new Texture2D(background.width, background.height, TextureFormat.RGB24, false);	
		int startX = 0;
		int startY = background.height - watermark.height;
			
		for (int x = startX; x < background.width; x++)
		{
			for (int y = startY; y < background.height; y++)
			{
				Color bgColor = background.GetPixel(x, y);
				Color wmColor = watermark.GetPixel(x - startX, y - startY);

				Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);
					
				tempTex.SetPixel(x, y, final_color);
			}
		}
			
		tempTex.Apply (false);
		return tempTex;
	}

	public static Texture2D CombineTextures(Texture2D aBaseTexture, Texture2D aToCopyTexture)
	{
		int aWidth = aBaseTexture.width;
		int aHeight = aBaseTexture.height;
		Texture2D aReturnTexture = new Texture2D(aWidth, aHeight, TextureFormat.RGBA32, false);
		
		Color[] aBaseTexturePixels = aBaseTexture.GetPixels();
		Color[] aCopyTexturePixels = aToCopyTexture.GetPixels();
		Color[] aColorList = new Color[aBaseTexturePixels.Length];
		int aPixelLength = aBaseTexturePixels.Length;
		
		for(int p = 0; p < aPixelLength; p++)
		{
			aColorList[p] = Color.Lerp(aBaseTexturePixels[p], aCopyTexturePixels[p], aCopyTexturePixels[p].a);
		}
		
		aReturnTexture.SetPixels(aColorList);
		aReturnTexture.Apply(false);
		
		return aReturnTexture;
	}
	
	//Show again the user cursor (To avoid saving it to the texture)
	void ShowCursor(){	
		saving = false;
	}

	////////////////// PUBLIC METHODS //////////////////

	public void SetBrushMode(Painter_BrushMode brushMode){ //Sets if we are painting or placing decals
		mode = brushMode;
		brushCursor.GetComponent<SpriteRenderer> ().sprite = brushMode == Painter_BrushMode.DECAL ? cursorDecal : cursorPaint;
	}
	

	public void SetBrushSize(float newBrushSize){ //Sets the size of the cursor brush or decal
		brushSize = newBrushSize;
		brushCursor.transform.localScale = Vector3.one * brushSize;
	}

	public void UpdateBrushColor(Color color){
		brushColor = color;
	}

	public void SetBrushObject(GameObject _obj){
		brushObj = _obj;
	}

	public void SaveTextureToFile2(){
		SaveTexture ();
	/*	Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);		
		tex.ReadPixels (new Rect (0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
		tex.Apply ();
		StartCoroutine (SaveTextureToFile(tex) );
		 */
	}


	public void ResetPaint(){
		//UPDATE: this should destroy/delete all brushes so it will be reset (bot paint and decal)
		UpdateSkinColor (Color.white);
	}

	/// <summary>
	/// Updates the color of the skin. If color, removed the texture
	/// </summary>
	public void UpdateSkinColor(Color color)
	{
		Material baseMat = canvasBaseMaterial.GetComponent<Renderer> ().materials [0]; 
		Texture2D textMat = new Texture2D (64, 64);
		Color[] colroArray =  textMat.GetPixels();
		
		for(var i = 0; i < colroArray.Length; ++i)
		{
			colroArray[i] = color;
		}
		
		textMat.SetPixels( colroArray );
		
		textMat.Apply();
		baseMat.mainTexture = textMat;

	}
	
	
	/// <summary>
	/// Updates the skin texture. just retain the color, will be ovveried by text
	/// </summary>
	public void UpdateSkinTexture(Texture _text){
		Material baseMat = canvasBaseMaterial.GetComponent<Renderer> ().materials [0]; 
		baseMat.color = Color.white; 
		baseMat.mainTexture = _text;

	}


	public void UpdateDecal(Sprite _sprite){

		//TODO: with decals,
		//MAKE this different to paint.. instead it should be painted as per brush, then each painted decal, must be referenced so it can be moved and scaled.
		//before it is saved and applied to the object.

		cursorDecal = _sprite;
	}




	////////////////// OPTIONAL METHODS //////////////////

	#if !UNITY_WEBPLAYER 
		IEnumerator SaveTextureToFile(Texture2D savedTexture){		
			brushCounter=0;
			string fullPath=System.IO.Directory.GetCurrentDirectory()+"\\UserCanvas\\";
			System.DateTime date = System.DateTime.Now;
			string fileName = "CanvasTexture.png";
			if (!System.IO.Directory.Exists(fullPath))		
				System.IO.Directory.CreateDirectory(fullPath);
			var bytes = savedTexture.EncodeToPNG();
			System.IO.File.WriteAllBytes(fullPath+fileName, bytes);
			Debug.Log ("<color=orange>Saved Successfully!</color>"+fullPath+fileName);
			yield return null;
		}
	#endif
}
