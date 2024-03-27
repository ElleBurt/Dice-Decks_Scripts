using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using System.Text.RegularExpressions;



public class GenMap : MonoBehaviour
{   

    [Header("General")]
    public int encounterCount;
    public Vector2 miniBossCount;
    public Vector2 mapRangeX;
    public Vector2 mapRangeZ;
    public Vector2 mapRows;
    public Vector2 iconsPerRow;
    public Vector2 connectionsPerIcon;
    public float rowPadding;
    public float iconSpacing;
    public float restrictiveDistance;
    public float iconSize;
    


    [Header("Icons")]
    //list of icon templates
    public List<IconTemplate> iconTemplates = new List<IconTemplate>();

    //specific icons that cant be randomly selected
    public IconTemplate startIcon;
    public IconTemplate bossIcon;
    public IconTemplate encounterIcon;
    public IconTemplate dashIcon;

    

    //the prefab to add the decal to
    public GameObject decalPrefab;

    //Dictionary that stores the connections between icons
    public Dictionary<GameObject,List<GameObject>> connectedIcons = new Dictionary<GameObject,List<GameObject>>();

    [Header("Dashes")]
    //spacing between them
    public float lineSpacing;



    [Header("Trees")]
    //list of tree positions
    private List<Vector3> allMapEnviromentalsPos = new List<Vector3>();
    //treefab
    public GameObject treefab;

    GameController gameController;

    void Start(){
        gameController = FindObjectOfType<GameController>();
    }

    public void HighlightPaths(Transform LastIconTransform){

        foreach(Transform dash in LastIconTransform){
            Material highlightedDash = new Material(dash.gameObject.GetComponent<DecalProjector>().material);
            highlightedDash.color = Color.red;
            highlightedDash.SetColor("_EmissiveColor", Color.red * 5000f);
            dash.gameObject.GetComponent<DecalProjector>().material = highlightedDash;
        }
    }   

    public void IconGeneration(){
        //get a number of rows from the given bounds
        int numOfRows = Mathf.CeilToInt(Random.Range(mapRows.x,mapRows.y));
        //get the space in which the rows will be held
        int mapZDiferential = Mathf.CeilToInt(mapRangeZ.y - mapRangeZ.x);
        //get the hight of each rows bounds
        int rowDifference = mapZDiferential / numOfRows;

        //gets a list of which rows will contain encounters
        List<int> encounters = EncounterRows(numOfRows); 

        //sets the bounds of where the icons can spawn within each row (z-range)
        Vector2 rowBounds = new Vector2(-(rowDifference/2) + rowPadding , (rowDifference/2) - rowPadding);

        //loops through all the rows we want to add
        for(int i = 0; i < numOfRows; i++){

            //list to store all positions needed to spawn icons in
            List<Vector3> allIconPositions = new List<Vector3>(); 
            
            //makes a new row empty to hold the future icons
            GameObject row = new GameObject($"Row{i}");
            row.transform.position = transform.position;
            row.transform.SetParent(transform);

            //if row is the start or the end then we only need one icon, otherwise we can take a random about within the given bounds
            int numOfIcons = (i == 0 || i == numOfRows-1) ? 1 : Mathf.CeilToInt(Random.Range(iconsPerRow.x,iconsPerRow.y));
            
            //generates icon positions based off of amount above
            while(numOfIcons > 0){
                Vector3 iconPos = generateIconPosition(i,rowBounds,rowDifference,allIconPositions);
                allIconPositions.Add(iconPos);
                numOfIcons--;
            }

            //loops all icon positions
            foreach(Vector3 iconPos in allIconPositions){

                //makes new decal game object
                GameObject icon = GameObject.Instantiate(decalPrefab,iconPos,Quaternion.identity);

                //ternary to check if the row is the starting point or the end point
                IconTemplate icoTemp = (i == 0) ? startIcon : (i == numOfRows-1) ? bossIcon : (encounters.Contains(i)) ? encounterIcon : iconTemplates[Random.Range(0,iconTemplates.Count)];
               
                icon.transform.rotation = Quaternion.Euler(90,0,0);
                //sets parent to the column
                icon.transform.SetParent(row.transform);
                //sets the relevant name
                icon.name = icoTemp.name;
                icon.transform.tag = icoTemp.tag;

                //gets the decal projector
                DecalProjector Dp = icon.GetComponent<DecalProjector>();

                Dp.size = new Vector3(iconSize, iconSize, 10);

                //creates new material blueprint
                Material newMat = new Material(Dp.material);
                
                //sets the local scripts size var to the current size of the decal projector
                icon.GetComponent<mapDecals>().size = Dp.size;
                
                //sets the relevant base map and alpha
                newMat.SetTexture("_MaskMap", icoTemp.iconAlpha);
                newMat.SetTexture("_EmissiveColorMap", icoTemp.iconAlpha);
                newMat.SetTexture("_BaseColorMap", icoTemp.iconColor);

                //starts coroutine to fade the decals in on top of the map
                StartCoroutine(FadeFactor(Dp));

                //sets decal projectors material to the new material
                Dp.material = newMat;
            }
        }

        //sets the starting icon position in the game controller
        gameController.lastIconTransform = GameObject.FindGameObjectsWithTag("Start")[0].transform;

        //cals draw lines method
        DrawLines();

    }

    //create a list of all the rows that will include the encounters
    private List<int> EncounterRows(int numOfRows){

        //creates the relative spacing based off of the amount of rows and encounters
        int encounterSpacing = Mathf.CeilToInt(numOfRows / encounterCount);

        //creates a temp list to return when done
        List<int> rowsWithEncounters = new List<int>();

        //set the first row to have an encounter, in this case its always row 2
        int startingRow = 2;

        //while the lists contents are lower than the required amount of encounters - we add more
        while(rowsWithEncounters.Count < encounterCount){
            rowsWithEncounters.Add(startingRow);
            startingRow += encounterSpacing;
        }

        //return the list
        return rowsWithEncounters;
    }

    //used to generate icon positions within the give range on the given row, making sure no overlapping occurs
    private Vector3 generateIconPosition(int Row, Vector2 rowBounds, int rowDifference, List<Vector3> allIconPositions){

        //generates random position within given bounds
        Vector3 iconPos = new Vector3(Random.Range(mapRangeX.x,mapRangeX.y),0,mapRangeZ.x + (rowDifference * Row+1) + Random.Range(rowBounds.x,rowBounds.y));

        //simple bool to toggle if it intersects an existing icon
        bool posExists = false;

        //check if intersecting and sets the bool to the relative result
        foreach(Vector3 existingIconPos in allIconPositions){
            if(Vector3.Distance(iconPos, existingIconPos) < iconSpacing || Vector3.Distance(iconPos, existingIconPos) > restrictiveDistance){
                posExists = true;
                break;
            }
        }

        //if it is intersecting then we need to recurse the method
        if (posExists){
            return generateIconPosition(Row,rowBounds,rowDifference,allIconPositions);
        }else{
            //otherwise return the position
            return iconPos;
        }
    }





    //draws the lines between icons
    void DrawLines(){

        //list of icons
        List<GameObject> allIcons = new List<GameObject>();

        //adds all icons to the list
        allIcons.AddRange(GameObject.FindGameObjectsWithTag("MapIcon"));
        allIcons.AddRange(GameObject.FindGameObjectsWithTag("Start"));

        //loops all icons in the scene
        foreach(GameObject decal in allIcons){
           
            //gets the transform, its row, and its column
            Transform decalTransform = decal.transform;

            //gets current row number
            int currentRow = int.Parse(Regex.Replace(decalTransform.parent.name, @"\D",""));

            if(currentRow < transform.childCount-1){
                
                //if the current row is either the start or the row before the end then all icons need connections
                if(currentRow == 0 || currentRow == transform.childCount-2){
                    foreach(Transform icon in transform.Find($"Row{currentRow+1}")){
                        RenderLine(decalTransform,icon);

                        if(connectedIcons.ContainsKey(decal)){
                            connectedIcons[decal].Add(icon.gameObject);
                        }else{
                            List<GameObject> tmpList = new List<GameObject>();
                            tmpList.Add(icon.gameObject);
                            connectedIcons.Add(decal,tmpList);
                        }
                    }
                }else{
                    //if not calculate the ones that need connections
                    CalcConnections(decalTransform,currentRow);
                }
                
            }
            
            

            
        }

        foreach(GameObject decal in allIcons){

            if(decal.GetComponent<mapDecals>().connections == 0 && !decal.CompareTag("Start")){

                int currentRow = int.Parse(Regex.Replace(decal.transform.parent.name, @"\D",""));

                Transform lastRow = transform.Find($"Row{currentRow-1}");
                List<(Transform, float)> distances = new List<(Transform, float)>();

                foreach(Transform lastdecal in lastRow){
                    float dist = Vector3.Distance(lastdecal.position,decal.transform.position);
                    distances.Add((lastdecal,dist));
                }

                Transform lastIcon = distances.OrderBy(tf => tf.Item2).Take(1).ToList()[0].Item1;

                RenderLine(lastIcon,decal.transform);


            }
        }
        //now we can access all the icons and lines; we can generate the surrounding environment
        generateEnvironment();
    }

    //calculates which lines to render
    void CalcConnections(Transform Icon, int currentRow){

        //creates a new dictionary for each icon in the next row and its distance from the current icon
        Dictionary<Transform,float> currentIconsConnections = new Dictionary<Transform,float>();

        //add the appropriate values to the dictionary
        foreach(Transform nextIcon in transform.Find($"Row{currentRow+1}")){
                                 
            float distance = Vector3.Distance(Icon.position, nextIcon.position);

            currentIconsConnections[nextIcon] = distance;
                                  
        }

        //orders by distance and then by existing connections, then takes the first 1 or 2
        var connections = currentIconsConnections.OrderBy(tf => tf.Value).OrderBy(tf => tf.Key.GetComponent<mapDecals>().connections).Take(Mathf.CeilToInt(Random.Range(connectionsPerIcon.x,connectionsPerIcon.y))).ToList();
        GameObject decal = Icon.gameObject;

        //renders the line and then adds it to the dict
        foreach(var connection in connections){
            RenderLine(Icon,connection.Key);

            if(connectedIcons.ContainsKey(decal)){
                connectedIcons[decal].Add(connection.Key.gameObject);
            }else{
                List<GameObject> tmpList = new List<GameObject>();
                tmpList.Add(connection.Key.gameObject);
                connectedIcons.Add(decal,tmpList);
            }
        }


    }

    //simply draws a line from current icon to the next icon
    void RenderLine(Transform current, Transform next){

        next.gameObject.GetComponent<mapDecals>().connections++;

        //sets the direction in which the icon is
        Vector3 heading = (next.position - current.position).normalized;
        //sets the distance to the icon
        float distance = Vector3.Distance(current.position, next.position);

        //placeholder angle
        float angle = heading.x*65;

        
        //loops through the distance divided by the line spacing
        for(float dash = 0; dash < distance; dash += lineSpacing ){

            //sets the position and offsets based on heading
            Vector3 dashPos = current.position + dash * heading;

            //checks if the distance between the dash and the next icon is less than 2 and if its greater than 1 from the starting icon
            if(Vector3.Distance(dashPos, next.position) > 2f && Vector3.Distance(dashPos, current.position) > 1f){

                //instantiates the new decal, sets the rotation to the angle stated above, position to the dashPos with a small offset and sets the parent to the starting icon
                GameObject decal = GameObject.Instantiate(decalPrefab, dashPos + new Vector3(Random.Range(-0.3f,0.3f),0,0), Quaternion.identity);
                decal.transform.rotation = Quaternion.Euler(90,angle,0);
                decal.transform.SetParent(current);
                decal.name = dashIcon.name;

                //gets the box collider component of the decal and disables it, so there is no mouse interactions
                decal.GetComponent<BoxCollider>().enabled = false;

                //gets the projector
                DecalProjector Dp = decal.GetComponent<DecalProjector>();

                //copies material as new
                Material newMat = new Material(Dp.material);

                //makes the decals smaller than the icons would be
                Dp.size = new Vector3(iconSize/2f, iconSize/2f, 10);

                //sets the size on the local script
                decal.GetComponent<mapDecals>().size = Dp.size;
                
                //sets the materials
                newMat.SetTexture("_MaskMap", dashIcon.iconAlpha);
                newMat.SetTexture("_EmissiveColorMap", dashIcon.iconAlpha);
                newMat.SetTexture("_BaseColorMap", dashIcon.iconColor);

                //fades in
                StartCoroutine(FadeFactor(Dp));
                
                //sets the new material
                Dp.material = newMat;
            }
            
        }
        
    }

    //fades the icons in
    IEnumerator FadeFactor(DecalProjector Dp){
        
        float fadeNum = 0.0f;
        Dp.fadeFactor = fadeNum;
        yield return new WaitForSeconds(3f);

        while(Dp.fadeFactor < 1){
            fadeNum += 0.01f;
            Dp.fadeFactor = fadeNum;
            yield return new WaitForSeconds(0.01f);
        }
    }

    //fades the icons out
    IEnumerator FadeFactorReverse(DecalProjector Dp){
        
        float fadeNum = 1.0f;
        Dp.fadeFactor = fadeNum;

        while(Dp.fadeFactor > 0){
            fadeNum -= 0.04f;
            Dp.fadeFactor = fadeNum;
            yield return new WaitForSeconds(0.01f);
        }
    }

    //hides or shows the icons
    public void displayIcons(bool show){

        List<GameObject> icons = new List<GameObject>();
        icons.AddRange(GameObject.FindGameObjectsWithTag("MapIcon"));
        icons.AddRange(GameObject.FindGameObjectsWithTag("Start"));

        if(show){
            foreach(GameObject dp in icons){
                DecalProjector decal = dp.GetComponent<DecalProjector>();
                StartCoroutine(FadeFactor(decal));
            }
            ReDrawEnvironment();
        }else{
            foreach(GameObject dp in icons){
                DecalProjector decal = dp.GetComponent<DecalProjector>();
                StartCoroutine(FadeFactorReverse(decal));
            }
            
        }
        
    }

    //randomly distributes enviromental objects around the map avoiding any icons or dashes
    private void generateEnvironment(){
        for(int i = 0; i < 300; i++){

            //sets the bounds of the map and randomly selects a position in it
            float Xpos = Random.Range(-40f,46f);

            //because i made the map curved when layed out for whatever reason, this sets the offset on the y relative to how far it is from the center line
            float ajustedXPos = Xpos > 3f ? Xpos - 5f : Xpos;
            float Ypos = Mathf.Abs(ajustedXPos)/17.5f - 2.8f;
            float Zpos = Random.Range(27.5f,155.5f);

            //bool to state is selected pos is colliding with a decal
            bool hitObstacle = false;

            //sets the random position
            Vector3 spawnPos = new Vector3(Xpos, Ypos, Zpos);

            List<GameObject> points = new List<GameObject>();

            points.AddRange(GameObject.FindGameObjectsWithTag("MapIcon"));
            points.AddRange(GameObject.FindGameObjectsWithTag("Enviro"));
            points.AddRange(GameObject.FindGameObjectsWithTag("Start"));

            //checks all decals in the scene to see if colliding
            foreach(GameObject point in points){
                if(Vector3.Distance(spawnPos, point.transform.position) < 3.5f){
                    hitObstacle = true;
                    break;
                }
            }
            
            //if bool was not triggered then we can spawn the object
            GameObject spawnObj;
            Quaternion spawnRot;

            spawnObj = treefab;
            spawnRot = Quaternion.Euler(0,0,0);
        

            if(!hitObstacle){
                GameObject tree = GameObject.Instantiate(spawnObj, spawnPos + new Vector3(0,-10, 0), Quaternion.identity * spawnRot);
                StartCoroutine(moveUp(tree,tree.transform.position));
                allMapEnviromentalsPos.Add(tree.transform.position);
            }
        }

    }

    //draws the generated environment above whenever its needed again
    public void ReDrawEnvironment(){
        foreach(Vector3 treePos in allMapEnviromentalsPos){
            Quaternion spawnRot = Quaternion.Euler(0,0,0);
            GameObject tree = GameObject.Instantiate(treefab, treePos, Quaternion.identity * spawnRot);
            StartCoroutine(moveUp(tree,tree.transform.position));
        }
    }

    //clears the environment above whenever needed
    public void clearEnviro(){
        foreach(GameObject tree in GameObject.FindGameObjectsWithTag("Enviro")){
            StartCoroutine(moveDown(tree, tree.transform.position));
        }
    }

    //move up sequence for the enviromental objects
    IEnumerator moveUp(GameObject tree, Vector3 startPos) {

        yield return new WaitForSeconds(2f);

        while(Vector3.Distance(tree.transform.position, (startPos + new Vector3(0,11,0))) > 0.1f){

            tree.transform.position = Vector3.Lerp(tree.transform.position, (startPos + new Vector3(0,11,0)), 7f * Time.deltaTime);

            yield return null;
        }
    }
    //move down sequence for the enviromental objects
    IEnumerator moveDown(GameObject tree, Vector3 startPos) {

        while(Vector3.Distance(tree.transform.position, (startPos - new Vector3(0,15,0))) > 0.1f){

            tree.transform.position = Vector3.Lerp(tree.transform.position, (startPos - new Vector3(0,15,0)), 2f * Time.deltaTime);

            yield return null;
        }
        
        Destroy(tree);
    }
}
