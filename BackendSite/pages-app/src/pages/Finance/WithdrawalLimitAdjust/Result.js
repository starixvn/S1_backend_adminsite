import React, {useState, useEffect} from 'react';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';


const numberFormat = (number)=>{
    return number_format(number_toFixed(number, 2));
}

const currencyFormat = (currencyId)=>{
    return window.i18n["lbl_Currency"+currencyId]
}

const defCustInfoCols = {
    siteName:{dataColumns:'siteName', displayText:'SiteName'},
    userName:{dataColumns:'userName', displayText:'UserName'},
    currency:{dataColumns:'currency', displayText:'Currency', format:currencyFormat},
    balance:{dataColumns:'balance', displayText:'Balance', format:numberFormat},
    differenceAmount:{dataColumns:'differenceAmount', displayText:'Difference Amount', format:numberFormat},
    eligibleWithdrawalAmount:{dataColumns:'eligibleWithdrawalAmount', displayText:'Eligible Withdrawal Amount', format:numberFormat}
}

const Panel = (props)=>{
    let rows = Object.keys(defCustInfoCols).map((key, idx)=>{
        let col = defCustInfoCols[key];
        return(
            <tr key={`result-${idx}`}>
                <th scope="row">{col.displayText}</th>
                <td>{col.format ? col.format(props.CustInfo[col.dataColumns]) : props.CustInfo[col.dataColumns]}</td>
            </tr>
        )
    });

    return(
        <>
            <hr />
            <h4 className="page-name">Cust Info</h4>
            <table className="table table-detail">
                <tbody>
                    {rows}
                </tbody>
            </table>
        </>
    )
}

export default Panel;