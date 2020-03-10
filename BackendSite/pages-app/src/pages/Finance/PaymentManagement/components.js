import React from "react";

export const ElemType = {
    selectBox:'selectBox',
    inputBox:'inputBox',
    switchBtn:'switchBtn',
    actionBtn:'actionBtn'
}

export const Status = {
    1:'open',
    2:'disable'
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

export const OuterPanel = (props)=>{
    let id = `l_${props.Id}`;
    return(
        <div className="panel mt-20">
            <a className={`panel-title collapsed`} data-toggle="collapse" href={`#${id}`} aria-expanded="false">
                {props.HeaderContent}
            </a>
            {props.OuterContent}
            <div className="panel-body">
                <div className={`collapse`} id={id}>
                    {props.Content}
                </div>
            </div>
        </div>
    )
}

export const SwitchBtn = (props)=>{
    return props.Title ? (
        <>
            <label style={{marginRight:'10px'}}>{props.Title}:</label>
            <div className="switch__container">
                <input id={props.SwitchBtnKey} className="switch switch--flat" type="checkbox" checked={props.checked} onChange={props.onChange}/>
                <label htmlFor={props.SwitchBtnKey}></label>
            </div>
        </>
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
        <input className={props.className ?? ''} type={props.format} defaultValue={props.value} onChange={onChange}/>
    )
}
