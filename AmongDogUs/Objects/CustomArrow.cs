namespace AmongDogUs.Objects;

internal class CustomArrow
{
    internal float per = 0.925f;
    internal SpriteRenderer image;
    internal GameObject arrow;

    private static Sprite sprite;
    internal static Sprite GetSprite()
    {
        if (sprite) return sprite;
        sprite = Helpers.LoadSpriteFromTexture2D(ModAssets.Arrow, 200f);
        return sprite;
    }

    internal CustomArrow(Color color)
    {
        arrow = new GameObject("Arrow")
        {
            layer = 5
        };
        image = arrow.AddComponent<SpriteRenderer>();
        image.sprite = GetSprite();
        image.color = color;
    }

    internal void Update()
    {
        Vector3 target = Vector3.zero;
        Update(target);
    }

    internal void Update(Vector3 target, Color? color = null)
    {
        if (arrow == null) return;

        if (color.HasValue) image.color = color.Value;

        Camera main = Camera.main;
        Vector2 vector = target - main.transform.position;
        float Num = vector.magnitude / (main.orthographicSize * per);
        image.enabled = (double)Num > 0.3;
        Vector2 vector2 = main.WorldToViewportPoint(target);
        if (Between(vector2.x, 0f, 1f) && Between(vector2.y, 0f, 1f))
        {
            arrow.transform.position = target - (Vector3)vector.normalized * 0.6f;
            float Num2 = Mathf.Clamp(Num, 0f, 1f);
            arrow.transform.localScale = new Vector3(Num2, Num2, Num2);
        }
        else
        {
            Vector2 vector3 = new(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
            float orthographicSize = main.orthographicSize;
            float Num3 = main.orthographicSize * main.aspect;
            Vector3 vector4 = new(Mathf.LerpUnclamped(0f, Num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
            arrow.transform.position = main.transform.position + vector4;
            arrow.transform.localScale = Vector3.one;
        }

        LookAt2d(arrow.transform, target);
    }

    private static void LookAt2d(Transform transform, Vector3 target)
    {
        Vector3 vector = target - transform.position;
        vector.Normalize();
        float Num = Mathf.Atan2(vector.y, vector.x);
        if (transform.lossyScale.x < 0f)
            Num += 3.1415927f;
        transform.rotation = Quaternion.Euler(0f, 0f, Num * 57.29578f);
    }

    private static bool Between(float value, float min, float Max)
    {
        return value > min && value < Max;
    }
}