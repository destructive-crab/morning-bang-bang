namespace leditor.root;

public class LayerData : LEditorDataUnit
{
    public override bool ValidateExternalDataChange()
    {
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
    }
}