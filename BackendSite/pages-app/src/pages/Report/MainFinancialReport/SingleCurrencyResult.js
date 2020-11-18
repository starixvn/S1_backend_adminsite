import React from "react";
import moment from 'moment';
import { number_format, number_toFixed } from '../../../module/Formatting/NumberFormat';

const dataColumnDefs = () => {
    return [
        {
            columnTitle: 'Date',
            dataColumn: "date"
        },
        {
            columnTitle: 'Currency',
            dataColumn: "isoCode"
        },
        {
            columnTitle: 'Deposit',
            dataColumn: "deposit",
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            columnTitle: 'Withdrawal',
            dataColumn: "withdrawal",
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            columnTitle: 'Bonus',
            dataColumn: "bonus",
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            columnTitle: 'Rebate',
            dataColumn: "rebate",
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            columnTitle: 'Adjustment',
            dataColumn: "adjustment",
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            columnTitle: 'TurnOver(Stake)',
            dataColumn: "turnOverStake",
        body:{
            attr:{
                class:"text-right"
            },
            format:numberFormat
        }
        },
        {
            columnTitle: 'TurnOver(ValidBet)',
            dataColumn: "turnOverValidBet",
        body:{
            attr:{
                class:"text-right"
            },
            format:numberFormat
        }
        },
        {
            columnTitle: 'WinLoss',
            dataColumn: "winLoss",
        body:{
            attr:{
                class:"text-right"
            },
            format:numberFormat
        }
        },
    ]
}

const Panel = (props) => {
    const columnDefs = dataColumnDefs();
    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} scope="col" className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
                {element.columnTitle}
            </th>
        );
    });
    
    let bodyRows = props.Datas.map((data, idx) => {
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`} className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}>
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    return (
        <div className="table">
            <div className="table-options">
                <button type="button" className="btn btn-approved" onClick={props.ExportToExcel}>Export Excel</button>
            </div>
            <table className="table table-striped">
                <thead>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                </tbody>
            </table>
        </div>
    )
}

const numberFormat = (number) => {
    return number_format(number_toFixed(number, 2));
}

export default Panel;