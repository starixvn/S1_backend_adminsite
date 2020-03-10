import React from "react";
import {ActionButtons, SwitchBtn, SelectBox, InputBox, ElemType, AmountRule, InputBoxFormat}  from './components';

export const CurrencylInfoHeader = (props)=>{
    return(
        <span style={{'color':'white'}}>{props.Name}</span>
    );
}

const columnDefs = (props)=>{
    return [
        {
            dataColumn:'payCurrencyId', columnTitle:'Currency'
        },
        {
            dataColumn:'minAmount', columnTitle:'MinAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    Format:InputBoxFormat.number
                }
            }
        },
        {
            dataColumn:'maxAmount', columnTitle:'MaxAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    Format:InputBoxFormat.text
                }
            }
        },
        {
            dataColumn:'amountRule', columnTitle:'AmountRule',
            body:{
                EditMode:{
                    ElemType: ElemType.selectBox,
                    ElemClass: 'custom-select',
                    Options:[...Object.keys(AmountRule).map(key=>{return {value:key, displayText:AmountRule[key]}})]
                }
            }
        },
        {
            dataColumn:'amountShow', columnTitle:'AmountShow',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    Format:InputBoxFormat.text
                }
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

export const CurrencyResultTable = (props)=>{
    let columns = columnDefs();

    let headers = columns.map((col, idx)=>{
        let key = `${props.idxArr.join('-')}-${idx}`;
        return(
            <th key={`h-${key}`} scope="col" className={col.header && col.header.attr && col.header.attr.className ? col.header.attr.className : ''}>{col.columnTitle}</th>
        );
    });

    let bodyRows = props.CurrencyList.map((data, cIdx)=>{
        let key = `${props.idxArr.join('-')}-${cIdx}`;

        let rows = columns.map((col, idx)=>{
            let body_content = null;
            if(col.body && col.body.SpecialItem){
                if(col.body.SpecialItem === ElemType.actionBtn){
                    body_content = (
                        <ActionButtons 
                            isEdit={data.isEdit} 
                            idxArr={[...props.idxArr, cIdx]} 
                            ClickEdit={props.ClickEdit}
                            ClickCancel={props.ClickCancel}
                            ClickSave={props.ClickSave}
                        />
                    );
                }
            }else{
                if(data.isEdit && col.body && col.body.EditMode){
                    switch (col.body.EditMode.ElemType) {
                        case ElemType.selectBox:
                            body_content = (
                                <SelectBox
                                    dataColumn={col.dataColumn}
                                    idxArr={[...props.idxArr, cIdx]} 
                                    onChange={props.EditField}
                                    options={col.body.EditMode.Options}
                                    selectedValue={data.editValue ? data.editValue[col.dataColumn].value : ""}
                                    className={col.body.EditMode.ElemClass}
                                />
                            );
                            break;

                        case ElemType.inputBox:
                                body_content = (
                                    <InputBox 
                                        dataColumn={col.dataColumn}
                                        idxArr={[...props.idxArr, cIdx]} 
                                        onChange={props.EditField}
                                        value={data.editValue ? data.editValue[col.dataColumn] : ""}
                                        className={col.body.EditMode.ElemClass}
                                        format={col.body.EditMode.Format}
                                    />
                                )
                            break;

                        case ElemType.switchBtn:
                                body_content = (
                                    <SwitchBtn 
                                        SwitchBtnKey={[...props.idxArr, cIdx].join('-')}
                                        checked={data.editValue ? data.editValue[col.dataColumn].value===1 : false}
                                        onChange={props.EditField.bind(null, data.editValue[col.dataColumn].value===1 ? 2 : 1, col.dataColumn, [...props.idxArr, cIdx])}
                                    />
                                )
                            break;

                        default:
                            break;
                    }
                }else{
                    body_content = data[col.dataColumn] && data[col.dataColumn].displayText ? data[col.dataColumn].displayText : data[col.dataColumn];
                }
            }

            return(
                <td key={`b-${key}-${idx}`} className={col.body && col.body.attr && col.body.attr.className ? col.body.attr.className : ''}>
                    {body_content}
                </td>
            );
        });

        return <tr key={`tr-${key}`}>{rows}</tr>
    })

    return(
        <table className="table table-striped">
            <thead>
                <tr>
                    {headers}
                </tr>
            </thead>
            <tbody>
                {bodyRows}
            </tbody>
        </table>
    )
}

// export const CurrencylInfoContent = (props)=>{
//     return <ResultTable columnDefs={columnDefs()} {...props} />
// }