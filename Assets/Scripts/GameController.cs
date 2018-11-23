using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public enum CellType { Bear, Deer, Duck, Frog, Dog, Mouse, Pig, Cat, Panda, Rabbit }

    #region Properties
    public int Score
    {
        get
        {
            return m_score;
        }

        set
        {
            m_score = value;
            OnScoreChanged(value);
        }
    }
    #endregion

    #region Events

    public event System.Action<int> OnScoreChanged;
    public event System.Action<Combo> OnComboAppearChanged;

    #endregion

    [Header("Spawn settings:")]
    [SerializeField] Vector2 m_map_size;
    [SerializeField] Vector2 m_tile_offset;
    [Space]
    [SerializeField] Transform m_parent;
    [Space]
    [SerializeField] GameObject m_cell_pref;
    [Space]
    [Space]
    [Tooltip("Count of unique cells. Add more sprites to set more than 10"), Range(3, 10)]
    [SerializeField] int m_variety;
    [Space]
    [SerializeField] public List<Sprite> m_sprites;

    [Space]
    [SerializeField] Camera m_cam;

    [Header("Runtime:")]
    [Space]
    [SerializeField] List<Cell> m_cells = new List<Cell>();

    [Space]
    [SerializeField] int m_score;


    [SerializeField] List<Combo> combos;

    [Space]
    [SerializeField] Cell m_drag_enter;


    void Start()
    {
        for (int i = 0; i < m_map_size.x; i++)
        {
            for (int j = 0; j < m_map_size.y; j++)
            {
                var new_cell = Instantiate(m_cell_pref, new Vector3(i * m_tile_offset.x, j * m_tile_offset.y, 0), Quaternion.identity, m_parent).GetComponent<Cell>();

                new_cell.name = $"x: {i} y: {j}";

                var tile_type = Random.Range(0, m_variety);

                new_cell.TileType = (CellType)tile_type;
                new_cell.Coords = new Vector2(i, j);
                m_cells.Add(new_cell);

                var e_trigger = new_cell.gameObject.AddComponent<EventTrigger>();

                #region Events

                UnityAction<BaseEventData> begin_drag = (base_data) =>
                {
                    //Debug.Log("BeginDrag", gameObject);
                    TintNeighbours(GetNeighbours(m_cells, new_cell), Color.green);
                };
                UnityAction<BaseEventData> drag = (base_data) =>
                {
                    var pointer_event_data = base_data as PointerEventData;

                    //Debug.Log($"Drag Event -> Pointer entered object: {eventData.pointerEnter.name}", eventData.pointerEnter);
                    m_drag_enter = pointer_event_data.pointerEnter ? pointer_event_data.pointerEnter.GetComponent<Cell>() : null;

                };
                UnityAction<BaseEventData> end_drag = (base_data) =>
                {
                    Debug.Log("End Drag Event", gameObject);
                    TintNeighbours(GetNeighbours(m_cells, new_cell), Color.white);

                    if (m_drag_enter && Mathf.Abs(Vector2.Distance(new_cell.Coords, m_drag_enter.Coords)) <= 1)
                    {
                        SwapCells(m_drag_enter, new_cell);
                        StartCoroutine(UI.Timer(.6f, () => {
                        if (CheckCombination(out combos, m_cells, m_map_size))
                        {
                            var copy = new List<Combo>(combos); //prevent exception from clearing collection while coroutine is working

                            StartCoroutine(RaiseComboEvent(copy, 1.6f));

                            foreach (var item in combos)
                            {
                                item.cells.ForEach(x => ChangeCellType(x));
                            }
                            combos.Clear();
                            //Debug.Log($"Combo : {combo}");
                            m_drag_enter = null;
                        }
                        else
                        {
                            Debug.Log("No combo, swapping back...");
                            SwapCells(m_drag_enter, new_cell);
                        }
                        }));
                    }
                };

                var begin_drag_tr = new EventTrigger.Entry() { eventID = EventTriggerType.BeginDrag };
                var drag_tr = new EventTrigger.Entry() { eventID = EventTriggerType.Drag };
                var end_drag_tr = new EventTrigger.Entry() { eventID = EventTriggerType.EndDrag };

                end_drag_tr.callback.AddListener(end_drag);
                begin_drag_tr.callback.AddListener(begin_drag);
                drag_tr.callback.AddListener(drag);



                e_trigger.triggers.Add(begin_drag_tr);
                e_trigger.triggers.Add(drag_tr);
                e_trigger.triggers.Add(end_drag_tr);

                #endregion
            }
        }

        m_cam.transform.position = new Vector3(m_map_size.x / 4, m_map_size.y / 4, -3f);
        m_cam.orthographicSize = m_map_size.y / 3;

        RemoveComboLines();
    }

    public bool CheckCombination(out List<Combo> combo_list, List<Cell> cells, Vector2 map_size)
    {
        combo_list = new List<Combo>();

        for (int i = 0; i < map_size.x; i++)
        {
            var hor_combos = FindLineCombos(cells.FindAll(x => x.Coords.x == i), Combo.ComboType.Vertical, Color.blue);

            combo_list.AddRange(hor_combos);
        }

        for (int j = 0; j < map_size.y; j++)
        {
            var vert_combos = FindLineCombos(cells.FindAll(x => x.Coords.y == j), Combo.ComboType.Horizontal, Color.red);

            combo_list.AddRange(vert_combos);

        }
        //TODO: add "Cross" type finding
        //Debug.Log($"Check Combo: {is_combo}");
        return combo_list.Count > 0;
    }

    List<Combo> FindLineCombos(List<Cell> list, Combo.ComboType type, Color mark)
    {
        List<Combo> combo_list = new List<Combo>();

        var combo_counter = 1;  // include start cell
        var cur_id = list[0].TileType;
        var temp_combo = new List<Cell>()
            {
                list[0]
            };

        for (int z = 0; z < list.Count - 1; z++)
        {

            if (cur_id == list[z + 1].TileType)
            {
                combo_counter++;

                if (combo_counter >= 3)
                {
                    list[z - 1 >= 0 ? z - 1 : z].Color = mark;
                    list[z].Color = mark;
                    list[z + 1].Color = mark;
                }
            }
            else
            {
                if (combo_counter >= 3)
                {
                    combo_list.Add(new Combo()
                    {
                        cells = new List<Cell>(temp_combo),
                        combo_type = type,
                        cell_type = temp_combo[0].TileType
                    });
                    Debug.Log($"Line combo: x{combo_counter}");
                }
                cur_id = list[z + 1].TileType;
                temp_combo.Clear();

                combo_counter = 1;
            }
            temp_combo.Add(list[z + 1]);
        }
        return combo_list;
    }

    public List<Cell> GetNeighbours(List<Cell> cells, Cell cell)
    {
        var left = new Vector2(cell.Coords.x - 1, cell.Coords.y);
        var right = new Vector2(cell.Coords.x + 1, cell.Coords.y);
        var top = new Vector2(cell.Coords.x, cell.Coords.y + 1);
        var bot = new Vector2(cell.Coords.x, cell.Coords.y - 1);

        return cells.FindAll(x => x.Coords == left || x.Coords == right || x.Coords == top || x.Coords == bot);
    }

    public static void TintNeighbours(List<Cell> neighbours, Color color)
    {
        neighbours.ForEach(x => x.Color = color);
    }

    public IEnumerator RaiseComboEvent(List<Combo> combos, float delay)
    {
        foreach (var item in combos)
        {
            OnComboAppearChanged(item);
            Score += (item.cells.Count - 2) * 2;
            yield return new WaitForSeconds(delay);
        }
    }

    void ChangeCellType(Cell cell, bool animate = true)
    {
        int new_type;
        do
        {
            new_type = Random.Range(0, m_variety);

        } while (cell.TileType == (CellType)new_type);

        //Debug.Log($"Change type {cell.Coords} from {cell.TileType} to {(CellType)new_type}");
        if (animate)
        {
            StartCoroutine(UI.ProgressTimer(.25f, (prog, delta) => cell.transform.localScale = Vector3.one * (1 - prog),
                    () => StartCoroutine(UI.ProgressTimer(.25f, (prog, delta) => cell.transform.localScale = Vector3.one * (prog)))));

        }
        cell.TileType = (CellType)new_type;
        cell.Color = Color.white;
    }

    void RemoveComboLines()
    {
        var list_combo = new List<Combo>();
        var iter_count = 0;
        while (CheckCombination(out list_combo, m_cells, m_map_size))
        {
            foreach (var item in list_combo)
            {
                item.cells.ForEach(x=>ChangeCellType(x,false));
            }
            iter_count++;
            Debug.Log($"Without combos in {iter_count} iterations");
        } 
    }

    public void SwapCells(Cell cell1, Cell cell2, bool animate = false)
    {
        Debug.Log($"Swap Cells.  {cell1.name} and {cell2.name}");
        var pos1 = cell1.transform.position;
        var pos2 = cell2.transform.position;

        FindObjectOfType<GameController>().StartCoroutine(UI.ProgressTimer(.5f, (prog, delta) =>
        {
            cell1.transform.position = Vector3.Lerp(pos1, pos2, prog);
            cell2.transform.position = Vector3.Lerp(pos2, pos1, prog);
        }, () =>
        {
            cell1.transform.position = pos1;
            cell2.transform.position = pos2;

            var temp_id = cell1.TileType;
            cell1.TileType = cell2.TileType;
            cell2.TileType = temp_id;

        }));
    }

    [System.Serializable]
    public class Combo
    {
        public enum ComboType { Vertical, Horizontal, Cross }

        public ComboType combo_type;

        public CellType cell_type;

        public List<Cell> cells;
    }

    [ContextMenu("Test Combo")]
    void TestCombo()
    {
        var list_combo = new List<Combo>();

        if (CheckCombination(out list_combo, m_cells, m_map_size))
        {
            Debug.LogErrorFormat("There are combo");
        }
        else
        {
            Debug.Log("No combo");
        }

    }
}
