import React from "react";

export const ElemType = {
    selectBox:'selectBox',
    inputBox:'inputBox',
    switchBtn:'switchBtn',
    actionBtn:'actionBtn'
}

export const Status = {
    1:'Open',
    2:'Disable'
}

export const InputBoxFormat = {
    'text':'text',
    'number':'number'
}

export const Type = {
    1:'Online Banking',
    2:'Mobile Pay'
}

export const Mode = {
    1:'開新視窗，將html內容置入',
    2:'開新視窗，導頁至該Url',
    3:'將 url 以QrCode 呈現'
}

export const AmountRule = {
    1:'Integer',
    2:'Decima',
    3:'Fixed'
}

export const PanelClass = {
    level1_mt20: "panel level-1 mt-20",
    level2_mt05: "panel level-2 mt-05",
    mt5: "panel mt-05"
}

export const statusFormat = (status)=>{
    let className = status == 1 ? 'status-active' : 'status-suspend';
    return <span className={`status ${className}`}>{Status[status]}</span>;
}

export const OuterPanel = (props)=>{
    let header_class = "";
    let body_class = "";
    if(props.isCollaps){
        header_class = "panel-title";
        body_class = "collapse show";
    }else{
        header_class = "panel-title collapsed";
        body_class = "collapse";
    }
    return(
        <div className={props.PanelClass}>
            <a style={{"justifyContent": "space-between"}} className={header_class} onClick={props.HandleCollaps.bind(null, props.idxArr)}>
                {props.HeaderContent}
            </a>
            {props.OuterContent}
            <div className="panel-body">
                <div className={body_class}>
                    {props.Content}
                </div>
            </div>
        </div>
    )
}

export const SwitchBtn = (props)=>{
    return props.Title ? (
        <div style={{marginRight: "25px"}}>
            <span style={{padding: "0 10px", color:'white'}}>{props.Title}:</span>
            <div className="switch__container">
                <input id={props.SwitchBtnKey} className="switch switch--flat" type="checkbox" checked={props.checked} onChange={props.onChange}/>
                <label htmlFor={props.SwitchBtnKey}></label>
            </div>
        </div>
    ) : (
        <div className="switch__container">
            <input id={props.SwitchBtnKey} className="switch switch--flat" type="checkbox" checked={props.checked} onChange={props.onChange}/>
            <label htmlFor={props.SwitchBtnKey}></label>
        </div>
    )
}

export const ActionButtons = (props)=>{
    return props.isEdit ? (
        <div className="button-action">
            <button className="btn btn-save" onClick={props.ClickSave.bind(null, props.idxArr)}>
                <i className="icon-save"></i>
            </button>
            <button className="btn btn-icon-cancel" onClick={props.ClickCancel.bind(null, props.idxArr)}>
                <i className="icon-cancel"></i>
            </button>
        </div>
    ) : (
        <div className="button-action">
            <button className="btn btn-edit" onClick={props.ClickEdit.bind(null, props.idxArr)}>
                <i className="icon-edit"></i>
            </button>
        </div>
    )
}

export const SelectBox = (props)=>{
    const onChange = (e)=>{
        if(props.onChange){
            props.onChange(e.target.value, props.dataColumn, props.idxArr);
        }
    }

    let optionRows = props.options.map((item, idx)=>{
        return(
            <option key={`${props.idxArr.join('-')}-${props.dataColumn}-${idx}`} value={item.value}>{item.displayText}</option>
        )
    });
    return(
        <select className={props.className ?? ''} defaultValue={props.selectedValue} onChange={onChange}>
            {optionRows}
        </select>
    )
}

export const InputBox = (props)=>{
    const onChange = (e)=>{
        if(props.onChange){
            
            props.onChange(e.target.value, props.dataColumn, props.idxArr);
        }
    }
    return(
        <input className={props.className ?? ''} type={props.attrFormat} defaultValue={props.value} onChange={onChange}/>
    )
}
