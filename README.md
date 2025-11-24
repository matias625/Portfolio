# Portfolio
Portfolio to show Unity Engine code used in itch.io

For Cameras I used Cinemachine. To change between them used Priority.Value. 

SceneControl (GameObject)
 └── SceneControl_MoveTest (Script) {  
    Character List = assign 
    Camera Controller
    Character Tabs 
    Character Stats = 
    Character Meters = 
}

CameraController (GO)
 └── Camera_Controller (Script) { 
    listCameras = list of cameras that can be used by MoveBasic/Aim Controllers. Write cameras name with "Isometric", "Orbital", "3rd" or "Shoulder" to be used by MoveBase_Controller and inherited }

MoveBasic (GO)
 └── MoveBasic_Controller (Script){ 
    availableCameras = list of labels of cameras that can be used by this Controller (Note: when search use string.Contains(label)).
    useCameraView = if want to convert input by camera view.
    }

MoveAim (GO)
 └── MoveAim_Controller (Script){
    AvailableCameras = same as MoveBasic.
    UseCameraView = recomend true to convert input by camera view.
    PointerArrow = its a transform that will move to your mouse position.
 }

ManagerInput (GO)
 ├── PlayerInputManager (Component)
 ├── Input_Manager (Script)
 └── pf_Input_Player (GO / Prefab)
     └── PlayerInput (Component)

WaterManager (GO) { Only needed if use Move_Boat }
 └── WaterManager (Script) { Require a noise texture assigned. }

GridManager (GO) { Only needed if use Move/Dash/Jump _Grid }
 └── GridManager (Script)


CharacterA (GameObject)
 ├── Model { here add your model }
 ├── Move_Control (Script)
 ├── Rigidbody (Component)
 ├── DetectGround_{ RayCast if Basic or Aim, Collision if Sphere, -Rest Not Needed- } (Script)
 ├── Move
 │    ├── Move_{ Grid, Basic, Aim, Sphere, Boat } (Script)
 │    └── CapsuleCollider (Component)
 ├── Dash
 │    └── Dash_{ Grid, Basic, Aim, Sphere, -Boat not have- } (Script)
 └── Jump
      └── Jump_{ Grid, Basic, Aim, Sphere, -Boat not have- } (Script)


WaterDeformer (GO) { must be in position.y=0}
 ├── MeshRenderer (Component) { 
 │    Must be a plane with vertices inside, can be a quad but will not show much }
 ├── WaterMeshDeformer (Script) 
 └── FollowTarget (Script) { 
    used to follow Character with Boat. Follow on X and Z. }