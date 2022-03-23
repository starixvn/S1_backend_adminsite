export const InputBox = (props) => {
    return (
        <input type="text" defaultValue={props.DefaultValue} onChange={(e) => {props.OnChange(e.target.value)}} readOnly={props.Disabled} />
    )
}

export const SelectList = (props) => {
    let selectOptions = props.SelectOptions.map((item) => {
        return <option value={item.value}>{item.name}</option>;
    });
    return (
        <select className="form-control" value={props.Value} onChange={(e) => { props.OnChange(e.target.value)}} disabled={props.Disabled}>
            {selectOptions}
        </select>
    )
}

export const ActionButtons = (props) => {
    return props.IsEdit ? (
        <div className="button-action">
            <button className="btn btn-save" onClick={props.HandleUpdate} disabled={props.Disabled}>
                <i className="icon-save"></i>
            </button>
            <button className="btn btn-icon-cancel" onClick={props.HandleCancel} disabled={props.Disabled}>
                <i className="icon-cancel"></i>
            </button>
        </div>
    ) : (
            <div className="button-action">
                <button className="btn btn-edit" onClick={props.HandleEdit} disabled={props.Disabled}>
                    <i className="icon-edit"></i>
                </button>
            </div>
        )
}