import React from "react";

const dataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: '%',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Total Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Total Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Averange Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Success Rate(%)',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'FTD Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Averange Processing Time(d.HH:MM:SS)',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        }
    ]
}

const dataColumnDefs = (props) => {
    return [
        { 
            columnTitle: 'AccountName',
            dataColumn: 'accountName',
            body:{
                attr:{
                    bgcolor:"Bisque",
                    class:"text-center",
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
                top: "73.6px",
                left:"0",
                zIndex:"3"
            }
        },
        //Online Bank
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `onlineSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "onlineSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `onlinePrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "onlinePrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Mobile Bank
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `mobileSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "mobileSummary",
            
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `mobilePrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "mobilePrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
         //Local Bank
         ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `localSummary_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total In MYR',
            dataColumn: "localSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `localPrecent_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total %',
            dataColumn: "localPrecentSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Total Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Amount In MYR',
            dataColumn: "totalAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },

        //Total Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Count',
            dataColumn: "totalCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Averange Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `averangeAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Averange Amount IN MYR',
            dataColumn: "averangeAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Success Rate
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `successRate_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Success Rate',
            dataColumn: "successRateSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //FTD Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `ftdCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total FTD Count',
            dataColumn: "ftdCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Averange Processing Time
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `apTime_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Averange Processing Time',
            dataColumn: "apTimeSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
    ]
}

const dateDataGroupColumnDefs = (props) => {
    return [
        {
            columnTitle: 'Total Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Total Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Averange Amount',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'Success Rate(%)',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        },
        {
            columnTitle: 'FTD Count',
            colSpan: props.CurrencyList.length + 1,
            attr:{
                class:"text-center",
            },
            style:{
                position: "sticky",
                top: "36.8px",
                height:"36.8px",
                zIndex:"2"
            }
        }
    ]
}

const dateDataColumnDefs = (props) => {
    return [
        { 
            columnTitle: 'Date',
            dataColumn: 'date',
            body:{
                attr:{
                    bgcolor:"Bisque",
                    class:"text-center",
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
                top: "73.6px",
                left:"0",
                zIndex:"3"
            }
        },
        //Total Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Amount In MYR',
            dataColumn: "totalAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Total Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `totalCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Count',
            dataColumn: "totalCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Averange Amount
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `averangeAmount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Averange Amount IN MYR',
            dataColumn: "averangeAmountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //Success Rate
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `successRate_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total Success Rate',
            dataColumn: "successRateSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
        //FTD Count
        ...props.CurrencyList.map(curr=>{
            return({
                columnTitle: `${curr.code}`,
                dataColumn: `ftdCount_${curr.id}`,
                body:{
                    attr:{
                        class:"text-right",
                    },
                    style:{}
                },
                style:{
                    position: "sticky", 
                    top: "73.6px",
                    zIndex:"2"
                }
            })
        }),
        {
            columnTitle: 'Total FTD Count',
            dataColumn: "ftdCountSummary",
            body:{
                attr:{
                    class:"text-right",
                    bgcolor:"LightGray"
                },
                style:{}
            },
            style:{
                position: "sticky", 
                top: "73.6px",
                zIndex:"2"
            }
        },
    ]
}

const Panel = (props) => {
console.log(props);
    const groupColumnDefs = dataGroupColumnDefs(props);
    const columnDefs = dataColumnDefs(props);

     let groupHeaderRows = [];
     let headerRows = [];

     const dateGroupColumnDefs = dateDataGroupColumnDefs(props);
     const dateColumnDefs = dateDataColumnDefs(props);
 
      let dateGroupHeaderRows = [];
      let dateHeaderRows = [];

    groupColumnDefs.forEach((element, idx) => {
        groupHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                style={element.style ? element.style : ''}
                colSpan={element.colSpan}>
                {element.columnTitle}
            </th>
        );
    });

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
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

    props.DateDatas.forEach((data)=>{
        dateGroupColumnDefs.forEach((element, idx) => {
            dateGroupHeaderRows.push(
                <th key={`th-${element.dataColumn}-${idx}`}
                    scope="col"
                    className={element && element.attr && element.attr.class ? element.attr.class : ''} 
                    style={element.style ? element.style : ''}
                    colSpan={element.colSpan}>
                    {element.columnTitle}
                </th>
            );
        });
    });

    dateColumnDefs.forEach((element, idx) => {
        dateHeaderRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                style={element.style ? element.style : ''}>
                {element.columnTitle}
            </th>
        );
    });

    let dateBodyRows = props.DateDatas.map((data, idx) => {
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

    let dateTotalRows = props.DateTotalReuslt.map((data, idx) => {
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
            <div>
            </div>
            <div className="table-options">
                <button type="button" className="btn btn-approved ">Summary</button>
                <button type="button" className="btn btn-approved">Date</button>
                 <button type="button" className="btn btn-approved" onClick={props.ExportBtnDisabled ? null : props.ExportToExcel} disabled={props.ExportBtnDisabled}>Export Excel</button>
            </div>
            <table className="table table-striped" style={{width:"auto"}} hidden="true">
                <thead>
                    <tr>
                        <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}></th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Online Banking</th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Mobile Pay</th>
                        <th colSpan={(props.CurrencyList.length+1)*2} className="text-center" style={{position:"sticky", top:"0px" }}>Local Banking</th>
                        <th colSpan={(props.CurrencyList.length+1)*6} style={{position:"sticky", top:"0px" }}></th>
                    </tr>
                    <tr>
                        <th style={{ position: "sticky", top: "36.8px",left:"0",height:"36.8px",zIndex:"3"}}></th>
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
            <table className="table table-striped" style={{width:"auto"}}>
                <thead>
                    <tr>
                        <th style={{ position: "sticky", top: "0",left:"0",height:"36.8px",zIndex:"3"}}>Account</th>
                        <th colSpan={(props.CurrencyList.length+1)*6} style={{position:"sticky", top:"0px" }}></th>
                    </tr>
                    <tr>
                        <th style={{ position: "sticky", top: "36.8px",left:"0",height:"36.8px",zIndex:"3"}}></th>
                        {dateGroupHeaderRows}
                    </tr>
                    <tr>
                        {dateHeaderRows}
                    </tr>
                </thead>
                <tbody>
                    {/* {dateBodyRows}
                    {dateTotalRows} */}
                </tbody>
            </table>
        </div>
    )
}

export default Panel;