import React from "react";
import moment from 'moment';

const dataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Deposit',
            headerStartwith: 'deposit_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Withdrawal',
            headerStartwith: 'withdrawal_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Nett',
            headerStartwith: 'nett_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Bonus',
            headerStartwith: 'bonus_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Rebate',
            headerStartwith: 'rebate_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'Adjustment',
            headerStartwith: 'adjustment_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'TurnOver(Stake)',
            headerStartwith: 'turnOverStake_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'TurnOver(ValidBet)',
            headerStartwith: 'turnOverValidBet_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
        },
        {
            columnTitle: 'WinLoss',
            headerStartwith: 'winLoss_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
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
                        class:"text-right",
                        hidden:props.HideShowBtn
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
                dataColumn: `nett_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right"
                    },
                }
            })
        }),
        {
            columnTitle: 'Total in MYR',
            dataColumn: "nettSummary",
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

    const groupColumnDefsDefault = dataGroupColumnDefs(props);
    const columnDefsDefault = dataColumnDefs(props);

    let groupHeaderRows = [];
    let headerRows = [];

    let groupColumnDefs = [];
    let columnDefs = [];

    if(props.HidenTitles.hiddenTitlesList.length > 0)
    {

            columnDefsDefault.forEach(elem=>{
                
                let flag=true;
                
                props.HidenTitles.hiddenTitlesList.forEach(HiddenElem=>{
                    if(elem.dataColumn.startsWith(HiddenElem.Header))
                    {
                        flag=false;
                    }
                });

                if(flag)
                {
                    columnDefs.push(elem);
                }
            });
            
            groupColumnDefsDefault.forEach(elem=>{

                       let flag=true;

                        props.HidenTitles.hiddenTitlesList.forEach(HiddenElem=>{
                            if(elem.columnTitle===HiddenElem.Group)
                            {
                                flag=false;
                            }
                        });

                        if(flag)
                        {
                            groupColumnDefs.push(elem);
                        }
                        else
                        {
                            groupColumnDefs.push({
                                columnTitle: elem.columnTitle,
                                headerStartwith: elem.headerStartwith,
                                colSpan:  1,
                                btnText: 'show'
                            });

                        }
            });
        
    }
    else
    {
        columnDefs = columnDefsDefault;
        groupColumnDefs = groupColumnDefsDefault;
    }

    groupColumnDefs.forEach((element, idx) => {
        groupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''} colSpan={element.colSpan}
                bgcolor={element.bgcolor}>
                {element.columnTitle}
                <button type="button" className="btn btn-info pull-right" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button>
                {/* {<HideShowBtnElem
                onClick={props.HideShowBtn(element)}
                />} */}
            </th>
        );
    });


    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col"
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
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
                    bgcolor={element.body && element.body.attr && element.body.attr.bgcolor ? element.body.attr.bgcolor : ''}
                   >
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

export default Panel;