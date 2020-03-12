import React from "react";
import {CurrencyResultTable} from './CurrencyInfo';
import {ElemType, Mode, InputBoxFormat, Type, ActionButtons, SwitchBtn, SelectBox, InputBox}  from './components';

export const ChannelInfoHeader = (props)=>{
    return(
        <>
            <span style={{color:'white'}}>{props.Name}</span>
            <SwitchBtn Title="Status" SwitchBtnKey={props.idxArr.join('-')} checked={props.Status===1} onChange={props.EditField.bind(null, props.Status===1 ? 2 : 1, 'status', props.idxArr)}/>
        </>
    );
}

export const ChannelInfoContent = (props)=>{
    let {currencyList, ...channelInfo} = props;
    return(
        <div className="table">
            <ResultTable {...channelInfo} ContentOnClick={props.ContentOnClick}/>            
            <CurrencyResultTable 
                CurrencyList={currencyList} 
                idxArr={props.idxArr} 
                ClickEdit={props.ClickEdit}
                ClickCancel={props.ClickCancel}
                ClickSave={props.ClickSave}
                EditField={props.EditField}
            />
        </div>
    )
}

const columnDefs = (props)=>{
    return [
        {
            dataColumn:'type', columnTitle:'Type',
            body:{
                format:typeFormat,
                onClick:props.ContentOnClick
            }
        },
        {
            dataColumn:'sort', columnTitle:'Sort',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    AttrFormat:InputBoxFormat.number
                }
            }
        },
        {
            dataColumn:'mode', columnTitle:'Mode',
            body:{
                EditMode:{
                    ElemType: ElemType.selectBox,
                    ElemClass: 'custom-select',
                    Options:[...Object.keys(Mode).map(key=>{return {value:key, displayText:Mode[key]}})]
                },
                format:modeFormat
            }
        },
        {
            columnTitle:'Action',
            header:{
                attr:{
                    className:'width-85'
                }
            },
            body:{
                SpecialItem:ElemType.actionBtn
            }
        }
    ]
}

const typeFormat = (type, onClick)=>{
    return type === 1 ? <a style={{cursor:'pointer'}} data-toggle="modal" data-target="#ModalScrollable" onClick={onClick ? onClick : ()=>{}}><u>{Type[type]}</u></a> : Type[type];
}

const modeFormat = (mode)=>{
    return Mode[mode];
}

const ResultTable = (props)=>{
    let columns = columnDefs(props);
    let headers = [];
    let bodyRows = [];

    columns.forEach((col, idx)=>{
        let key = `${props.idxArr.join('-')}-${idx}`;
        headers.push(
            <th key={`h-${key}`} scope="col" className={col.header && col.header.attr && col.header.attr.className ? col.header.attr.className : ''}>{col.columnTitle}</th>
        );

        let body_content = null;
        if(col.body && col.body.SpecialItem){
            if(col.body.SpecialItem === ElemType.actionBtn){
                body_content = (
                    <ActionButtons 
                        isEdit={props.isEdit} 
                        idxArr={props.idxArr} 
                        ClickEdit={props.ClickEdit}
                        ClickCancel={props.ClickCancel}
                        ClickSave={props.ClickSave}
                    />
                );
            }
        }else{
            if(props.isEdit && col.body && col.body.EditMode){
                switch (col.body.EditMode.ElemType) {
                    case ElemType.selectBox:
                        body_content = (
                            <SelectBox
                                dataColumn={col.dataColumn}
                                idxArr={props.idxArr} 
                                onChange={props.EditField}
                                options={col.body.EditMode.Options}
                                selectedValue={props.editValue ? props.editValue[col.dataColumn] : ""}
                                className={col.body.EditMode.ElemClass}
                            />
                        );
                        break;

                    case ElemType.inputBox:
                            body_content = (
                                <InputBox 
                                    dataColumn={col.dataColumn}
                                    idxArr={props.idxArr} 
                                    onChange={props.EditField}
                                    value={props.editValue ? props.editValue[col.dataColumn] : ""}
                                    className={col.body.EditMode.ElemClass}
                                    format={col.body.EditMode.Format}
                                />
                            )
                        break;

                    default:
                        break;
                }
            }else{
                let func = col.body && col.body.format && col.body.onClick ? col.body.onClick.bind(null, col.dataColumn, props.idxArr) : null;
                body_content = col.body && col.body.format ? col.body.format(props[col.dataColumn], func) : props[col.dataColumn];
            }
        }

        bodyRows.push(
            <td key={`b-${key}`} className={col.body && col.body.attr && col.body.attr.className ? col.body.attr.className : ''}>
                {body_content}
            </td>
        );
    });

    return(
        <table className="table table-striped">
            <thead>
                <tr>
                    {headers}
                </tr>
            </thead>
            <tbody>
                <tr>
                    {bodyRows}
                </tr>
            </tbody>
        </table>
    )
}