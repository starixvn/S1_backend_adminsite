import React from 'react';

export default (props)=>{
    const OnChange = (e)=>{
        let value = e.target.value;
        let checked = value === '' || props.regex === undefined ? true : value.match(props.regex);
        if(checked){
            props.OnChange(value, props.dataColumn)
        }
    }
    return(
        <div className={props.ClassName ? props.ClassName : "input-item"}>
            {
                props.HideTitle === true ? null : <label className="label">{props.Title}</label>
            }
            <div className={`field${props.iptSubClass ? (' '+props.iptSubClass) : ''}`}>
                {
                    props.SubTitle ? <span className="bule-bold">{props.SubTitle}</span> : null
                }
                <textarea type={'text'} rows="8" value={props.value === null || props.value === undefined ? '' : props.value} onChange={OnChange} disabled={props.Disabled === true} className={props.inputClass ? props.inputClass : ''}/>
            </div>
        </div>
    )
}

