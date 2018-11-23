using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cell : MonoBehaviour
{
    #region Properties

    public Vector2 Coords
    {
        get
        {
            return m_coords;
        }
        set
        {
            m_coords = value;
        }
    }

    public Color Color { set { m_s_renderer.color = value; } }

    public GameController.CellType TileType
    {
        get
        {
            return m_tile_type;
        }

        set
        {
            m_tile_type = value;

            m_s_renderer.sprite = FindObjectOfType<GameController>().m_sprites[(int)value]; //! Remove FIND
        }
    }

    #endregion

    [SerializeField] GameController.CellType m_tile_type;

    [SerializeField] Vector2 m_coords;
    [SerializeField] SpriteRenderer m_s_renderer;

    public bool is_blocked;

    void Awake()
    {
        m_s_renderer = GetComponent<SpriteRenderer>();
    }
}


