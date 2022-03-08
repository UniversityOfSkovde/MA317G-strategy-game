# Strategy Game

## Installation
1. Make sure you have the following software installed:
* Git
* Git LFS
* Git Flow
2. Open Unity and open the Package Manager (located under `Window -> Package Manager`)
3. Press the `+` icon and select `Add Package From git URL...`
4. Enter the URL: `https://github.com/UniversityOfSkovde/MA317G-strategy-game.git` and press `Add`

## Usage

### Create a new map
To get started, create an empty game object and call it `Grid`. Press `Add Component`
and find the script `Grid` (not the built-in Unity script but one added by this package).

`Tile Prefab` should be set to a prefab of an object that will be instantiated for each tile
on the grid. There is one in the package. If you navigate to `Packages/Strategy Game/Runtime`
in the project browser, you an then drag-and-drop the `Piece`-prefab into the `Tile Prefab`
field in the inspector.

Change the `Tile Property Enum` to `Piece Property`. Your map should now be set up!

### Edit the map
Select one (or many) tiles in the scene view and set the type using the checkboxes in 
the `Grid Tile` component. The following types should be configurable:

* Obstacle
* Trap
* Portal
* Check Point
* Agent
* Terrain

### Using the map data from code
The grid data can be accessed from any script attached to the grid (the parent object).
Create a custom script, add it to the parent as a component and enter the following
code to see it in action:

```csharp
using System.Collections.Generic;
using Grid;
using UnityEngine;
using Vectors;

[ExecuteInEditMode]
[RequireComponent(typeof(Grid.Grid), typeof(VectorRenderer))]
public class Demo : MonoBehaviour {
    [SerializeField, HideInInspector] private Grid.Grid _grid;
    [SerializeField, HideInInspector] private VectorRenderer _vectors;
    
    void Start() {
        if (!TryGetComponent(out _grid)) {
            _grid = gameObject.AddComponent<Grid.Grid>();
        }
        
        if (!TryGetComponent(out _vectors)) {
            _vectors = gameObject.AddComponent<VectorRenderer>();
        }
    }
    
    void Update() {
        var size = _grid.Size;
        var selection = new List<Vector2Int>();
        var portals = new List<(Vector2Int From, Vector2Int To)>();
        
        for (var i = 0; i < size.x; i++) {
            for (var j = 0; j < size.y; j++) {
                var pos = new Vector2Int(i, j);
                if (_grid.GetTileProperty(pos, PieceProperty.Trap)) {
                    selection.Add(pos);
                }

                if (_grid.GetTileProperty(pos, PieceProperty.Portal)) {
                    var dest = _grid.GetTileData<Portal>(pos);
                    portals.Add((pos, new Vector2Int(dest.x, dest.y)));
                }
            }
        }

        if (selection.Count > 0) {
            using (_vectors.Begin()) {
                DrawSelection(selection, Color.red);
                foreach (var portal in portals) {
                    _vectors.Draw(
                        new Vector3(portal.From.x, 0, portal.From.y),
                        new Vector3(portal.To.x, 0, portal.To.y),
                        Color.cyan);
                }
            }
        }
    }
    
    private void DrawSelection(IEnumerable<Vector2Int> cells, Color color) {
        var edges = new HashSet<(Vector2Int l0, Vector2Int l1)>();
        
        foreach (var cell in cells) {
            var south = (cell, cell + Vector2Int.right);
            var west = (cell, cell + Vector2Int.up);
            var north = (cell + Vector2Int.up, cell + Vector2Int.one);
            var east = (cell + Vector2Int.right, cell + Vector2Int.one);
            if (!edges.Add(south)) edges.Remove(south);
            if (!edges.Add(west)) edges.Remove(west);
            if (!edges.Add(north)) edges.Remove(north);
            if (!edges.Add(east)) edges.Remove(east);
        }
        
        foreach (var edge in edges) {
            _vectors.Draw(
                new Vector3(edge.l0.x - .5f, 0, edge.l1.y - .5f),
                new Vector3(edge.l0.x - .5f, 0, edge.l1.y - .5f),
                color, 0.1f
            );
        }
    }
}
```

## Attribution
Some example assets are included for the purpose of illustrating the different
tile types. The meshes are made by Kenney and the full asset packs can be downloaded
from [Kenney.nl](https://kenney.nl) (available under public domain).

## License
```
MIT License

Copyright (c) 2022 Emil Forslund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```