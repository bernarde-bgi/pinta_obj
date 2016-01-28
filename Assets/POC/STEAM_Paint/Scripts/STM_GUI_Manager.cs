using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class STM_GUI_Manager : MonoBehaviour {


	public Slider sizeSlider;
	public TexturePainter painter;

	[SerializeField]
	private Texture _textCube1;
	[SerializeField]
	private Texture _textCube2;

	[SerializeField]
	private Sprite _decalSkull;
	[SerializeField]
	private Sprite _decalSmiley;
	
	//COLOR PICKER
	public HSVPicker picker;

	void Start(){
		picker.onValueChanged.AddListener(color =>
		                                  {
			UpdateBrushColor(color);
		});
	}


	private void UpdateBrushColor (Color color){
		painter.UpdateBrushColor (color);
	}

	public void SetBrushMode(int newMode){
		Painter_BrushMode brushMode =newMode==0? Painter_BrushMode.DECAL:Painter_BrushMode.PAINT; //Cant set enums for buttons :(
		string colorText=brushMode==Painter_BrushMode.PAINT?"orange":"purple";	
		painter.SetBrushMode (brushMode);
	}

	public void SetEraseMode(){
	//	painter.SetBrushMode (Painter_BrushMode.ERASE);
		painter.ResetPaint ();
	}

	public void SetDecalDesign(string decalName){
		Painter_BrushMode brushMode = Painter_BrushMode.DECAL;
		switch (decalName) {
			case "skull": 
				painter.UpdateDecal(_decalSkull);break;
			case "smiley":
				painter.UpdateDecal(_decalSmiley);break;
		}
		
		painter.SetBrushMode (brushMode);
	}

	

	public void SaveImage(){
		painter.SaveTextureToFile2 ();
	}
	

	public void UpdateSizeSlider(){
		painter.SetBrushSize(sizeSlider.value);
	}

	public void EnableEraseMode(){

	}

	public void Save(){

	}

	public void UpdateSkin(int skin_number){
		switch (skin_number) {
			case 0: 
				painter.UpdateSkinColor(Color.white);
				break;
			case 1: 
			painter.UpdateSkinColor(Color.red);
				break;
			case 2: 
			painter.UpdateSkinColor(Color.green);
				break;
			case 3: 
			painter.UpdateSkinTexture(_textCube1);
				break;
			case 4: 
			painter.UpdateSkinTexture(_textCube2);
				break;
			default: 
			painter.UpdateSkinColor(Color.white);
				break;
		}
	}

	public void UpdateDecal(string decalName){
		switch (decalName) {
		case "decalSkull":
			break;
		case "decalSmiley":break;
		}
	}


	void OnDestroy(){
		picker.onValueChanged.RemoveAllListeners ();
	}
}
