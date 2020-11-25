import React from "react";
import moment from 'moment';

const dataGroupColumnDefs = () => {
    return [
        {
            columnTitle: 'Deposit',
        },
        {
            columnTitle: 'Withdrawal',
        },
        {
            columnTitle: 'Bonus',
        },
        {
            columnTitle: 'Rebate',
        },
        {
            columnTitle: 'Adjustment',
        },
        {
            columnTitle: 'TurnOver(Stake)',
        },
        {
            columnTitle: 'TurnOver(ValidBet)',
        },
        {
            columnTitle: 'WinLoss',
        },
    ]
}

const dataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Date',
            dataColumn: "date"
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `deposit_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    }
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "depositSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `withdrawal_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "withdrawalSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `bonus_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "bonusSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `rebate_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "rebateSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `adjustment_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "adjustmentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverStake_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverStakeSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `turnOverValidBet_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "turnOverValidBetSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `winLoss_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "winLossSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
            }
        },
    ]
}

const Panel = (props) => {

    const groupColumnDefs = dataGroupColumnDefs();
    const columnDefs = dataColumnDefs(props);

    let groupHeaderRows = [];
    let headerRows = [];

    groupColumnDefs.forEach((element, idx) => {
        groupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''} colSpan={props.CurrencyList.length + 1}
                bgcolor={element.bgcolor}>
                {element.columnTitle}
            </th>
        );
    });

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
                <td key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    bgcolor={element.body && element.body.attr && element.body.attr.bgcolor ? element.body.attr.bgcolor : ''}>
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

    let totalRows = props.TotalResult.map((data, idx) => {
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <th key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    bgcolor="LemonChiffon">
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </th>
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
                <button type="button" className="btn btn-approved" onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th></th>
                        {groupHeaderRows}
                    </tr>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                    {totalRows}
                </tbody>
            </table>
        </div>
    )
}

// const numberFormat = (number) => {
//     return number_format(number_toFixed(number, 2));
// }

export default Panel;