import React from "react";
import {ActionButtons, SwitchBtn, SelectBox, InputBox, ElemType, AmountRule, InputBoxFormat}  from './components';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

export const CurrencylInfoHeader = (props)=>{
    return(
        <span style={{'color':'white'}}>{props.Name}</span>
    );
}

const columnDefs = (props)=>{
    return window.subFuncKeys && window.subFuncKeys.includes('f_pm_editAmountRule') ? [
        {
            dataColumn:'payCurrencyId', columnTitle:'Currency'
        },
        {
            dataColumn:'minAmount', columnTitle:'MinAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    AttrFormat:InputBoxFormat.number
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'maxAmount', columnTitle:'MaxAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    AttrFormat:InputBoxFormat.text
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'amountRule', columnTitle:'AmountRule',
            body:{
                EditMode:{
                    ElemType: ElemType.selectBox,
                    ElemClass: 'custom-select',
                    Options:[...Object.keys(AmountRule).map(key=>{return {value:key, displayText:AmountRule[key]}})],
                    SubElem:{
                        ElemType: ElemType.inputBox,
                        dataColumn:'amountShow',
                        AttrFormat:InputBoxFormat.text,
                        MatchParentValue:3
                    }
                },
                format:amountRuleFormat,
                subElem:{
                    dataColumn:'amountShow',
                    columnTitle:'AmountShow'
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
    ] : [
        {
            dataColumn:'payCurrencyId', columnTitle:'Currency'
        },
        {
            dataColumn:'minAmount', columnTitle:'MinAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    AttrFormat:InputBoxFormat.number
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'maxAmount', columnTitle:'MaxAmount',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    AttrFormat:InputBoxFormat.text
                },
                format:numberFormat
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
                            let currentValue = data.editValue ? data.editValue[col.dataColumn] : "";
                            let subElem = null;
                            if(col.body.EditMode.SubElem && col.body.EditMode.SubElem.MatchParentValue == currentValue){
                                subElem=(
                                    <InputBox 
                                        dataColumn={col.body.EditMode.SubElem.dataColumn}
                                        idxArr={[...props.idxArr, cIdx]} 
                                        onChange={props.EditField}
                                        value={data.editValue ? data.editValue[col.body.EditMode.SubElem.dataColumn] : ""}
                                        className={col.body.EditMode.SubElem.ElemClass}
                                        attrFormat={col.body.EditMode.SubElem.AttrFormat}
                                    />
                                )
                            }

                            body_content = (
                                <>
                                    <SelectBox
                                        dataColumn={col.dataColumn}
                                        idxArr={[...props.idxArr, cIdx]} 
                                        onChange={props.EditField}
                                        options={col.body.EditMode.Options}
                                        selectedValue={currentValue}
                                        className={col.body.EditMode.ElemClass}
                                    />
                                    {subElem}
                                </>
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
                                        attrFormat={col.body.EditMode.AttrFormat}
                                    />
                                )
                            break;

                        case ElemType.switchBtn:
                                body_content = (
                                    <SwitchBtn 
                                        SwitchBtnKey={[...props.idxArr, cIdx].join('-')}
                                        checked={data.editValue ? data.editValue[col.dataColumn]===1 : false}
                                        onChange={props.EditField.bind(null, data.editValue[col.dataColumn]===1 ? 2 : 1, col.dataColumn, [...props.idxArr, cIdx])}
                                    />
                                )
                            break;

                        default:
                            break;
                    }
                }else{
                    body_content = col.body && col.body.format ? col.body.format(data[col.dataColumn], col.body && col.body.subElem ? data[col.body.subElem.dataColumn] : null) : data[col.dataColumn];
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

const numberFormat = (number)=>{
    return number_format(number_toFixed(number, 2));
}

const amountRuleFormat = (amountRule, subData)=>{
    return subData ? `${AmountRule[amountRule]} - ${subData}` : AmountRule[amountRule];
}