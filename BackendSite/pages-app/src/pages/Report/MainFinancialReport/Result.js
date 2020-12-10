import React from "react";
import moment from 'moment';

const dataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Deposit',
            headerStartwith: 'deposit_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Withdrawal',
            headerStartwith: 'withdrawal_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Nett',
            headerStartwith: 'nett_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide'
            ,style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Bonus',
            headerStartwith: 'bonus_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Rebate',
            headerStartwith: 'rebate_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Adjustment',
            headerStartwith: 'adjustment_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'TurnOver(Stake)',
            headerStartwith: 'turnOverStake_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'TurnOver(ValidBet)',
            headerStartwith: 'turnOverValidBet_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'WinLoss',
            headerStartwith: 'winLoss_',
            colSpan: props.CurrencyList.length + 1,
            btnText: 'hide',
            style:{
                position: "sticky",
                top: "0",
                height:"36.8px",
                zIndex:"2"
            }
        },
    ]
}

const dataColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Date',
            dataColumn: "date",
            body:{
                attr:{
                    bgcolor:"Bisque"
                },
                style:{
                    position: "sticky",
                    top: "42.36px",
                    left:"0",
                    height:"36.8px",
                    zIndex:"1"
                }
            },
            style:{
                position: "sticky",
                top: "42.36px",
                left:"0",
                height:"36.8px",
                zIndex:"3"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `deposit_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                        hidden:props.HideShowBtn,
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "42.36px",
                    height:"36.8px",
                    zIndex:"2"
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
                style:{}
            },
            style:{
                position: "sticky", 
                top: "42.36px",
                height:"36.8px",
                zIndex:"2"
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
                                btnText: 'show',
                                style:elem.style
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
                colSpan={element.colSpan}
                style={element.style ? element.style : ''}>
                <div>
                    {element.columnTitle}
                    <button type="button" onClick={() => props.HideShowBtn(element)}>{element.btnText}</button>
                </div>
            </th>
        );
    });

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
            scope="col"
            style={element.style ? element.style : ''}>
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
                    style={element.body && element.body.style ? element.body.style : ''}
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
                    style={element.body && element.body.style ? element.body.style : ''}
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
                        <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}></th>
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