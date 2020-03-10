import React from "react";
import {ActionButtons, SwitchBtn, SelectBox, InputBox, ElemType}  from './components';

export const ResultTable = (props)=>{
    let columns = props.columnDefs;
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
                                selectedValue={props.editValue ? props.editValue[col.dataColumn].value : ""}
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

                    case ElemType.switchBtn:
                            body_content = (
                                <SwitchBtn 
                                    SwitchBtnKey={props.idxArr.join('-')}
                                    checked={props.editValue ? props.editValue[col.dataColumn].value===1 : false}
                                    onChange={props.EditField.bind(null, props.editValue[col.dataColumn].value===1 ? 2 : 1, col.dataColumn, props.idxArr)}
                                />
                            )
                        break;

                    default:
                        break;
                }
            }else{
                body_content = props[col.dataColumn] && props[col.dataColumn].displayText ? props[col.dataColumn].displayText : props[col.dataColumn];
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