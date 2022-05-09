import React from "react";
import {ElemType, SwitchBtn}  from './components';

if(process.env.NODE_ENV!=='production'){
    window.i18n = {"lbl_Currency2":"MYR","lbl_Currency4":"THB","lbl_Currency13":"CNY","lbl_Currency15":"IDR","lbl_Currency51":"VND","lbl_Currency61":"INR"};
}

const columnDefs = ()=>{
    return [
        {
            dataColumn:'bankCode', columnTitle:'BankCode'
        },
        {
            dataColumn:'bankName', columnTitle:'BankName'
        },
        {
            dataColumn:'currencyId', columnTitle:'Currency', 
            body:{
                format:currency_format
            }
        },
        {
            dataColumn:'status', columnTitle:'Status',
            body:{
                ElemType: ElemType.switchBtn,
                ElemClass: 'input-large'
            }
        }
    ]
}

export const BankInfoContent = (props)=>{
    let columns = columnDefs();
    let headers = columns.map((col, idx)=>{
        let key = `bankInfo-h-${idx}`;
        return(
            <th key={`h-${key}`} scope="col" className={col.header && col.header.attr && col.header.attr.className ? col.header.attr.className : ''}>{col.columnTitle}</th>
        );
    });

    if(props.BankList===null || props.BankList===undefined){
        return  <p>Loading...</p>
    }else{
        let bodyRows = props.BankList.length === 0 ? <tr><td>No Data.</td><td></td><td></td></tr> : props.BankList.map((data, cIdx)=>{
            let key = `bankInfo_${cIdx}`;

            let rows = columns.map((col, idx)=>{
                let body_content = null;

                if(col.body){
                    switch (col.body.ElemType) {
                        case ElemType.switchBtn:
                                body_content = (
                                    <SwitchBtn 
                                        SwitchBtnKey={[cIdx].join('-')}
                                        checked={data[col.dataColumn]===1}
                                        onChange={props.EditField.bind(null, data[col.dataColumn]===1 ? 2 : 1, col.dataColumn, [cIdx])}
                                    />
                                )
                            break;

                        default:
                            body_content = col.body && col.body.format ? col.body.format(data[col.dataColumn]) : data[col.dataColumn];
                            break;
                    }
                }else{
                    body_content = col.body && col.body.format ? col.body.format(data[col.dataColumn]) : data[col.dataColumn];
                }
                
                return(
                    <td key={`b-${key}-${idx}`} className={col.body && col.body.attr && col.body.attr.className ? col.body.attr.className : ''}>
                        {body_content}
                    </td>
                );
            });

            return <tr key={`tr-${key}`}>{rows}</tr>
        });
        
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
        );
    }
}

const currency_format = (currencyId)=>{
    return window.i18n["lbl_Currency"+currencyId]
}